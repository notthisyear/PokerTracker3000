using System;
using System.Globalization;
using System.Windows.Data;
using PokerTracker3000.GameSession.Sound;

namespace PokerTracker3000.WpfComponents.Converters
{
    public class SoundEffectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SoundEffect effect)
                return effect;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
