using System;

namespace PokerTracker3000.GameComponents
{
    [AttributeUsage(AttributeTargets.Field)]
    public class CurrencyAttribute : Attribute
    {
        public string Name { get; init; }

        public string CultureTag { get; init; }

        public string Code { get; init; }

        public CurrencyAttribute(string name, string cultureTag, string code)
        {
            Name = name;
            CultureTag = cultureTag;
            Code = code;
        }
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
