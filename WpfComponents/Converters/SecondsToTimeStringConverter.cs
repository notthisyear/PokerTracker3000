using System;
using System.Globalization;
using System.Windows.Data;

namespace PokerTracker3000.WpfComponents.Converters
{
    internal class SecondsToTimeStringConverter : IValueConverter
    {
        private const int SecondsPerMinute = 60;
        private const int MinutesPerHour = 60;
        private const int SecondsPerHour = SecondsPerMinute * MinutesPerHour;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int seconds)
                return 0;

            var numberOfHours = seconds / SecondsPerHour;
            seconds -= numberOfHours * SecondsPerHour;
            var numberOfMinutes = seconds / SecondsPerMinute;
            seconds -= numberOfMinutes * SecondsPerMinute;

            return $"{(numberOfHours > 0 ? $"{numberOfHours:D2}:" : "")}{numberOfMinutes:D2}:{seconds:D2}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
