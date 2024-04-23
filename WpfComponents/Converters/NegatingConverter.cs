using System;
using System.Windows.Data;

namespace PokerTracker3000.WpfComponents.Converters
{
    public class NegatingConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is double v ? -v : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is double v ? +v : value;
        }
    }
}
