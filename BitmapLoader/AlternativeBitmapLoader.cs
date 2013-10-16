using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using Splat;

namespace BitmapLoader
{
    /// <summary>
    /// Alternative BitmapLoader as suggested in https://github.com/paulcbetts/splat/pull/23
    /// </summary>
    public class AlternativeBitmapLoader : IBitmapLoader
    {
        public async Task<IBitmap> Load(Stream sourceStream, float? desiredWidth, float? desiredHeight)
        {
            return await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                async () => (await ByteArrayToBitmapImage(sourceStream, desiredWidth, desiredHeight)).FromNative());
        }

        private static async Task<BitmapImage> ByteArrayToBitmapImage(Stream stream, float? desiredWidth, float? desiredHeight)
        {
            var bitmapImage = new BitmapImage();

            if (desiredWidth != null)
            {
                bitmapImage.DecodePixelWidth = (int)desiredWidth;
                bitmapImage.DecodePixelHeight = (int)desiredHeight;
            }

            bitmapImage.SetSource(await ConvertToRandomAccessStream(stream));
            return bitmapImage;
        }

        private static async Task<InMemoryRandomAccessStream> ConvertToRandomAccessStream(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            var ras = new InMemoryRandomAccessStream();
            var writer = ras.AsStreamForWrite();

            await stream.CopyToAsync(writer);
            await writer.FlushAsync();

            ras.Seek(0);
            return ras;
        }

        public async Task<IBitmap> LoadFromResource(string resource, float? desiredWidth, float? desiredHeight)
        {
            return await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(async () =>
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(resource));
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    return await Load(stream.AsStreamForRead(), desiredWidth, desiredHeight);
                }
            });
        }

        public IBitmap Create(float width, float height)
        {
            return new WriteableBitmap((int)width, (int)height).FromNative();
        }
    }

    static class DispatcherMixin
    {
        public static Task<T> RunAsync<T>(this CoreDispatcher This, Func<Task<T>> func, CoreDispatcherPriority prio = CoreDispatcherPriority.Normal)
        {
            var tcs = new TaskCompletionSource<T>();

            This.RunAsync(prio, () =>
            {
                func().ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        tcs.SetException(t.Exception);
                    }
                    else
                    {
                        tcs.SetResult(t.Result);
                    }
                });
            });

            return tcs.Task;
        }
    }
}