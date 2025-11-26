using System;
using System.Globalization;
using System.Windows.Data;

namespace PokerTracker3000.WpfComponents.Converters
{
    public class PlayerImageWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == default ||
                values.Length != 3 ||
                values[0] is not double actualWidth ||
                values[1] is not double minWidth ||
                values[2] is not double maxWidth ||
                parameter is not double nominalRatio)
            {
                return 0.0;
            }
            return Math.Clamp(actualWidth * nominalRatio, minWidth, maxWidth);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
