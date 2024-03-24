using System;
using System.Globalization;
using System.Windows.Data;
using PokerTracker3000.Common;
using PokerTracker3000.GameComponents;

namespace PokerTracker3000.WpfComponents.Converters
{
    public sealed class ValueDisplayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 0)
                return string.Empty;

            if (values.Length == 1 && values[0] is CurrencyType currency)
                return currency.GetCustomAttributeFromEnum<CurrencyAttribute>().attr!.CurrencySymbol;

            if (values.Length != 2)
                return string.Empty;

            if (values[0] is not decimal v || values[1] is not CurrencyType c)
                return string.Empty;

            var (attr, e) = c.GetCustomAttributeFromEnum<CurrencyAttribute>();
            if (e != default)
                return string.Empty;

            return v.ToString(decimal.IsInteger(v) ? "C0" : "C", CultureInfo.CreateSpecificCulture(attr!.CultureTag));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
