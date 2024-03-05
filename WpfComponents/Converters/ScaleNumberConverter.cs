using System;
using System.Globalization;
using System.Windows.Data;

namespace PokerTracker3000.WpfComponents.Converters
{
    internal class ScaleNumberConverter : IValueConverter
    {
        private delegate double Operation(double x, double y);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v;
            if (value is double d)
                v = d;
            else if (value is int i)
                v = (double)i;
            else
                return value;

            if (parameter is not string s)
                return v;

            Operation op = s[0] switch
            {
                '+' => (x, y) => x + y,
                '-' => (x, y) => x - y,
                '*' => (x, y) => x * y,
                '/' => (x, y) => x / y,
                _ => (x, y) => x,
            };

            if (double.TryParse(s[1..], NumberStyles.Float, CultureInfo.InvariantCulture, out var sd))
                return op(v, sd);
            else if (int.TryParse(s[1..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var si))
                return op(v, (double)si);
            else
                return v;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
