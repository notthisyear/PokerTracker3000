using System;
using System.Globalization;
using System.Windows.Data;
using static PokerTracker3000.MainWindowFocusManager;

namespace PokerTracker3000.WpfComponents.Converters
{
    internal class FocusAreaToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string s)
                return false;
            if (value is not FocusArea area)
                return false;

            foreach (var part in s.Split('|'))
            {
                if (Enum.TryParse<FocusArea>(part, out var a) && a == area)
                    return true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }


    }
}
