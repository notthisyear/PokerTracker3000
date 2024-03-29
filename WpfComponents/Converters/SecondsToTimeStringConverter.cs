using System;
using System.Globalization;
using System.Text;
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

            if (parameter is string s && s.Equals("longformat", StringComparison.InvariantCultureIgnoreCase))
            {
                var hasHourString = numberOfHours > 0;
                var hasMinuteString = numberOfMinutes > 0;
                var hasSecondString = seconds > 0;

                StringBuilder sb = new();
                if (hasHourString)
                {
                    sb.Append(GetSuffixedStringIfLargerThanZero(numberOfHours, "h"));
                    if (hasMinuteString || hasSecondString)
                        sb.Append(", ");
                }

                if (hasMinuteString)
                {
                    sb.Append(GetSuffixedStringIfLargerThanZero(numberOfMinutes, "m"));
                    if (hasSecondString)
                        sb.Append(", ");
                }

                sb.Append(GetSuffixedStringIfLargerThanZero(seconds, "s"));
                return sb.ToString();
            }
            else
            {
                return $"{(numberOfHours > 0 ? $"{numberOfHours:D2}:" : "")}{numberOfMinutes:D2}:{seconds:D2}";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        private static string GetSuffixedStringIfLargerThanZero(int value, string suffix)
            => (value > 0 ? $"{value} {suffix}" : "");
    }
}
