using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PokerTracker3000.Common
{
    internal static class JsonExtensionMethods
    {
        public static (T?, Exception?) DeserializeJsonString<T>(this string serializedString,
                                                                bool convertSnakeCaseToPascalCase = false)
            => DeserializeJsonString<T>(serializedString, [], convertSnakeCaseToPascalCase);


        public static (T?, Exception?) DeserializeJsonString<T>(this string serializedString,
                                                                bool convertSnakeCaseToPascalCase = false,
                                                                JsonConverter? converter = default)
            => DeserializeJsonString<T>(serializedString, converter == default ? [] : [converter], convertSnakeCaseToPascalCase);

        public static (T?, Exception?) DeserializeJsonString<T>(this string serializedString,
                                                                List<JsonConverter> converters,
                                                                bool convertSnakeCaseToPascalCase = false)
        {
            if (string.IsNullOrEmpty(serializedString))
                return (default, new ArgumentNullException(nameof(serializedString)));

            var settings = new JsonSerializerSettings();
            if (convertSnakeCaseToPascalCase)
                settings.ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() };

            foreach (var converter in converters)
                settings.Converters.Add(converter);

            return serializedString.DeserializeJsonString<T>(settings);
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

        public static (string?, Exception?) SerializeToJsonString<T>(this T objectToSerialize,
                                                                    bool convertPascalCaseToSnakeCase = false,
                                                                    bool indent = false,
                                                                    bool ignoreNullValues = false,
                                                                    JsonConverter? converter = null)
            => SerializeToJsonString<T>(objectToSerialize, converter == default ? [] : [converter], convertPascalCaseToSnakeCase, indent, ignoreNullValues);

        public static (string?, Exception?) SerializeToJsonString<T>(this T objectToSerialize,
                                                                     List<JsonConverter> converters,
                                                                     bool convertPascalCaseToSnakeCase = false,
                                                                     bool indent = false,
                                                                     bool ignoreNullValues = false)
        {
            var settings = new JsonSerializerSettings();
            if (convertPascalCaseToSnakeCase)
                settings.ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() };

            foreach (var converter in converters)
                settings.Converters.Add(converter);

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
