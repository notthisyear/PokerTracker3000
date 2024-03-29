using System;
using System.Globalization;
using System.Windows.Data;

namespace PokerTracker3000.WpfComponents.Converters
{
    public class SecondsToTimestampConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int seconds)
                return string.Empty;

            DateTime ts;
            if (parameter is string s && s.Equals("utc", StringComparison.InvariantCultureIgnoreCase))
                ts = DateTime.UtcNow;
            else
                ts = DateTime.Now;

            ts = ts.AddSeconds(seconds);
            return $"{ts:t}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
