using System;
using System.Globalization;
using System.Windows.Data;
using PokerTracker3000.GameSession.Sound;

namespace PokerTracker3000.WpfComponents.Converters
{
    public class SoundConditionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Condition condition)
                return condition;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
