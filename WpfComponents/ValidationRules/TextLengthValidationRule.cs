using System.Globalization;
using System.Windows.Controls;

namespace PokerTracker3000.WpfComponents.ValidationRules
{
    public class TextLengthValidationRule : ValidationRule
    {
        public int Min { get; set; } = 0;

        public int Max { get; set; } = 100;

        public string FieldName { get; set; } = "Input";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is not string s)
                return new(false, "Could not parse input as string");

            if (s.Length < Min)
                return new ValidationResult(false, $"{FieldName} must be longer than {Min - 1} character{GetLastSIfNeeded(Min - 1)}");
            else if (s.Length > Max)
                return new ValidationResult(false, $"{FieldName} must be shorter than {Max + 1} character{GetLastSIfNeeded(Max + 1)}");
            else
                return ValidationResult.ValidResult;
        }

        private static string GetLastSIfNeeded(int x)
            => x == 1 ? "" : "s";
    }
}
