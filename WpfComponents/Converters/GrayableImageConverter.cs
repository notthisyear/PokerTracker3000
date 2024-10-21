using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PokerTracker3000.WpfComponents.Converters
{
    class GrayableImageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 2)
                return DependencyProperty.UnsetValue;

            if (values[0] is not string path || values[1] is not bool makeGray)
                return DependencyProperty.UnsetValue;

            return GetImage(path, makeGray) ?? DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static FormatConvertedBitmap? GetImage(string source, bool makeGray)
        {
            if (Uri.TryCreate(source, UriKind.Absolute, out var uri))
            {
                return new FormatConvertedBitmap(
                    new BitmapImage(uri),
                    makeGray ? PixelFormats.Gray8 : PixelFormats.Default,
                    null,
                    0);
            }
            return default;
        }
    }
}
