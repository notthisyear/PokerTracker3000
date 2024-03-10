using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using PokerTracker3000.Common;
using PokerTracker3000.GameComponents;

namespace PokerTracker3000.WpfComponents.Converters
{
    public sealed class ValueDisplayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!values.Any())
                return string.Empty;

            if (values.Length != 2)
                return string.Empty;

            if (values[0] is not decimal v || values[1] is not CurrencyType currency)
                return string.Empty;

            var (attr, e) = currency.GetCustomAttributeFromEnum<CurrencyAttribute>();
            if (e != default)
                return string.Empty;

            return v.ToString(decimal.IsInteger(v) ? "C0" : "C", CultureInfo.CreateSpecificCulture(attr!.CultureTag));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
