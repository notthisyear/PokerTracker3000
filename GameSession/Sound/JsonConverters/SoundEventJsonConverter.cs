using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace PokerTracker3000.GameSession.Sound
{
    public class SoundEventJsonConverter : JsonConverter<SoundEvent>
    {
        public override SoundEvent? ReadJson(JsonReader reader, Type objectType, SoundEvent? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var resolver = serializer.ContractResolver as DefaultContractResolver;
            var namingStrategy = resolver?.NamingStrategy;
            if (namingStrategy == default)
                return default;

            var jsonObject = JObject.Load(reader);
            SoundEvent result = new();

            foreach (var property in jsonObject.Properties())
            {
                if (property.Name.Equals(namingStrategy.GetPropertyName(nameof(SoundEvent.Name), false)))
                {
                    result.Name = property.Value.ToObject<string>(serializer) ?? string.Empty;
                }
                else if (property.Name.Equals(namingStrategy.GetPropertyName(nameof(SoundEvent.Conditions), false)))
                {
                    var conditions = property.Value.ToObject<List<Condition>>(serializer) ?? [];
                    foreach (var condition in conditions)
                        result.AddConditionToEvent(condition);
                }
                else if (property.Name.Equals(namingStrategy.GetPropertyName(nameof(SoundEvent.SoundEffectsOnEvent), false)))
                {
                    var effects = property.Value.ToObject<List<SoundEffect>>(serializer) ?? [];
                    foreach (var effect in effects)
                        result.AddSoundEffectToEvent(effect);
                }
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, SoundEvent? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
