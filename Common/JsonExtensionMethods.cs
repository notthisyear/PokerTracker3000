using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PokerTracker3000.Common
{
    internal static class JsonExtensionMethods
    {
        private static readonly JsonSerializerSettings s_settings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        public static (T?, Exception?) DeserializeJsonString<T>(this string serializedString, bool convertSnakeCaseToPascalCase = false)
        {
            if (string.IsNullOrEmpty(serializedString))
                return (default, new ArgumentNullException(nameof(serializedString)));
            return (convertSnakeCaseToPascalCase) ? serializedString.DeserializeJsonString<T>(s_settings) :
                                                    serializedString.DeserializeJsonString<T>(new JsonSerializerSettings());
        }

        public static (T?, Exception?) DeserializeJsonString<T>(this string serializedString, JsonSerializerSettings settings)
        {
            if (string.IsNullOrEmpty(serializedString))
                return (default, new ArgumentNullException(nameof(serializedString)));

            try
            {
                return (JsonConvert.DeserializeObject<T>(serializedString, settings), default);
            }
            catch (Exception e) when (e is JsonReaderException || e is JsonSerializationException)
            {
                return (default, e);
            }
        }

        public static (string?, Exception?) SerializeToJsonString<T>(this T objectToSerialize, bool convertPascalCaseToSnakeCase = false, bool indent = false, bool ignoreNullValues = false)
        {
            var settings = (convertPascalCaseToSnakeCase) ? s_settings : new JsonSerializerSettings();
            settings.NullValueHandling = ignoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include;

            try
            {
                return (JsonConvert.SerializeObject(objectToSerialize, settings: settings, formatting: indent ? Formatting.Indented : Formatting.None), default);
            }
            catch (Exception e) when (e is JsonReaderException || e is JsonSerializationException)
            {
                return (default, e);
            }
        }
    }
}
