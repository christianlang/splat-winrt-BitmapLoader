using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Splat;

namespace BitmapLoader
{
    public sealed partial class MainPage : Page
    {
        private readonly ObservableCollection<ImageSource> _images;

        public MainPage()
        {
            InitializeComponent();

            //Splat.BitmapLoader.Current = new AlternativeBitmapLoader();
            //Splat.BitmapLoader.Current = new OriginalBitmapLoader();

            _images = new ObservableCollection<ImageSource>();
            DataContext = _images;

            LoadResources();
        }

        private void LoadResources()
        {
            LoadResource("ms-appx:///Assets/Bitmaps/preview-83517-600x300.jpg");
            LoadResource("ms-appx:///Assets/Bitmaps/failure.bmp");
        }

        private async void LoadResource(string uri)
        {
            var bitmap = await Splat.BitmapLoader.Current.LoadFromResource(uri, 200, 200);
            _images.Add(bitmap.ToNative());
        }
    }
}
