using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PokerTracker3000.WpfComponents.Converters
{
    public class EmptyImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || (value is string s && string.IsNullOrEmpty(s)))
                return DependencyProperty.UnsetValue;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
