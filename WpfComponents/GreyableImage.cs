using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PokerTracker3000.WpfComponents
{
    public class GreyableImage : Image
    {
        private ImageSource? _sourceColor;
        private ImageSource? _sourceGreyscale;

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name.Equals(nameof(IsEnabled), StringComparison.InvariantCulture))
            {
                if (e.NewValue is not bool isEnabled)
                    return;

                Source = isEnabled ? _sourceColor : _sourceGreyscale;
            }
            else if (e.Property.Name.Equals(nameof(Source), StringComparison.InvariantCulture) &&
                     !ReferenceEquals(Source, _sourceColor) &&
                     !ReferenceEquals(Source, _sourceGreyscale))
            {
                _sourceColor = Source;
                _sourceGreyscale = CalculateGrayScaleImage() ?? _sourceColor;
            }
            base.OnPropertyChanged(e);
        }

        private ImageSource? CalculateGrayScaleImage()
        {
            if (Source == default)
                return default;

            var converter = TypeDescriptor.GetConverter(Source);
            var sourceAsString = converter.ConvertTo(Source, typeof(string)) as string;
            if (Uri.TryCreate(sourceAsString, UriKind.Absolute, out var uri))
                return new FormatConvertedBitmap(new BitmapImage(uri), PixelFormats.Gray8, null, 0);
            return default;
        }
    }
}
