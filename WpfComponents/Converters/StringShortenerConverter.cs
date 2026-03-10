using System;
using System.Globalization;
using System.Windows.Data;

namespace PokerTracker3000.WpfComponents.Converters
{
    public class StringShortenerConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == default)
                return string.Empty;

            if (values.Length == 2 &&
                values[0] is string s &&
                values[1] is int maxLength &&
                maxLength > 3)
            {
                return (s.Length <= maxLength) ? s : $"{s[..(maxLength - 3)]}...";
            }
            else
            {
                return values.Length > 0 ? values[0] : string.Empty;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
