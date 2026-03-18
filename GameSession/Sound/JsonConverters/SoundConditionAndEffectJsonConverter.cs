using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PokerTracker3000.GameSession.Sound.JsonConverters;

namespace PokerTracker3000.GameSession.Sound
{
    public class SoundConditionAndEffectJsonConverter<TType, TAttr> : JsonConverter<TType> where TType : class
                                                                                           where TAttr : ConditionAndEffectAttribute
    {
        private static readonly Dictionary<string, Type> s_typeMap =
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && typeof(TType).IsAssignableFrom(t))
                .Select(t => new
                {
                    Type = t,
                    Attr = t.GetCustomAttribute<TAttr>()
                })
                .Where(x => x.Attr != null)
                .ToDictionary(x => x.Attr!.Kind, x => x.Type, StringComparer.Ordinal);

        private static readonly Dictionary<string, Type> s_numberTypeMap = new(StringComparer.Ordinal)
        {
            { "int", typeof(int) },
            { "float", typeof(float) },
        };

        private readonly string _kindString = $"_{nameof(ConditionAndEffectAttribute.Kind).ToLower()}";
        private const string TypeString = "_type";

        public override TType? ReadJson(JsonReader reader, Type objectType, TType? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);

            if (!obj.TryGetValue(_kindString, StringComparison.Ordinal, out var kindToken))
                throw new JsonSerializationException($"Missing field '{_kindString}'");

            var kind = kindToken.ToObject<string>();

            if (kind == default || !s_typeMap.TryGetValue(kind, out var targetType))
                throw new JsonSerializationException($"Unknown condition kind: {kind}");

            // For generic type, we must extract the actual type before deserializing
            if (targetType.IsGenericType)
            {
                if (!obj.TryGetValue(TypeString, StringComparison.Ordinal, out var typeName))
                    throw new JsonSerializationException($"Missing field '{TypeString}' for generic type");

                var t = typeName.ToObject<string>();
                if (t == default || !s_numberTypeMap.TryGetValue(t, out var numberType))
                    throw new JsonSerializationException($"Unknown generic type: {t}");

                targetType = typeof(NumberCondition<>).MakeGenericType(numberType);
            }

            var s = GetSerializerWithoutConditionConverter(serializer);
            return (TType?)obj.ToObject(targetType, s);
        }

        public override void WriteJson(JsonWriter writer, TType? value, JsonSerializer serializer)
        {
            if (value == default)
            {
                writer.WriteNull();
                return;
            }

            var s = GetSerializerWithoutConditionConverter(serializer);
            writer.WriteStartObject();

            var attr = value.GetType().GetCustomAttribute<TAttr>();
            if (attr == default)
                throw new JsonSerializationException($"Missing required attribute {typeof(TAttr).FullName}");

            // Here, we add the extra property to the output
            writer.WritePropertyName(_kindString);
            writer.WriteValue(attr!.Kind);

            // We have subclasses with generics in some cases
            var t = value.GetType();
            if (t.IsGenericType)
            {
                var numberTypeName = s_numberTypeMap.First(x => x.Value == t.GetGenericArguments().First()).Key;
                writer.WritePropertyName(TypeString);
                writer.WriteValue(numberTypeName);
            }

            // Then, we write the rest
            foreach (var prop in JObject.FromObject(value, s).Properties())
                prop.WriteTo(writer);

            writer.WriteEndObject();
        }

        private static JsonSerializer GetSerializerWithoutConditionConverter(JsonSerializer serializer)
        {
            // Note: Because the value (some subclass of Condition) inherits the
            //       JsonConverter attribute of Condition, we recursivly call
            //       ourselves. Hence, we need to temporarily get rid of this
            //       converter so that we get out of the loop. One such solution
            //       is create a new serializer that doesn't have this converter.
            var temporarySerializer = new JsonSerializer
            {
                ContractResolver = serializer.ContractResolver,
                Formatting = serializer.Formatting,
                Culture = serializer.Culture
            };

            foreach (var converter in serializer.Converters)
            {
                if (converter is not SoundConditionAndEffectJsonConverter<TType, TAttr>)
                    temporarySerializer.Converters.Add(converter);
            }

            return temporarySerializer;
        }
    }
}
