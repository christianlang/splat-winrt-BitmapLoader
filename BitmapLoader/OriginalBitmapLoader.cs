using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Splat;

namespace BitmapLoader
{
    /// <summary>
    /// The original Load() method from 
    /// https://github.com/paulcbetts/splat/commit/b04b4894cdcc0a888bf5464ec7e60de4879cca3a
    /// </summary>
    public class OriginalBitmapLoader : IBitmapLoader
    {
        public async Task<IBitmap> Load(Stream sourceStream, float? desiredWidth, float? desiredHeight)
        {
            var source = new BitmapImage();

            if (desiredWidth != null) {
                source.DecodePixelWidth = (int)desiredWidth;
                source.DecodePixelHeight = (int)desiredHeight;
            }

            // NB: WinRT is dumb.
            var rwStream = new InMemoryRandomAccessStream();
            await sourceStream.CopyToAsync(rwStream.AsStreamForWrite());

            // the fixed version discussed in https://github.com/paulcbetts/splat/pull/23#issuecomment-26401327
            //var rwStream = new InMemoryRandomAccessStream();
            //var writer = rwStream.AsStreamForWrite();
            //await sourceStream.CopyToAsync(writer);
            //await writer.FlushAsync();
            //rwStream.Seek(0);

            await source.SetSourceAsync(rwStream);
            return source.FromNative();
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
            throw new System.NotImplementedException();
        }
    }
}