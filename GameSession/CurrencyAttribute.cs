using System;
using System.Globalization;

namespace PokerTracker3000.GameComponents
{
    [AttributeUsage(AttributeTargets.Field)]
    public class CurrencyAttribute(string name, string cultureTag, string code) : Attribute
    {
        public string Name { get; init; } = name;

        public string CultureTag { get; init; } = cultureTag;

        public string Code { get; init; } = code;

        public string CurrencySymbol => CultureInfo.CreateSpecificCulture(CultureTag).NumberFormat.CurrencySymbol;
    }

    public enum CurrencyType
    {
        [Currency("Svenska kronor", "sv-SE", "SEK")]
        SwedishKrona,

        [Currency("United States dollar", "en-US", "USD")]
        AmericanDollar,

        [Currency("Euro", "en-FR", "EUR")]
        Euro
    }
}
