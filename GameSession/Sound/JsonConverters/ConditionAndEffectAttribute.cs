using System;

namespace PokerTracker3000.GameSession.Sound.JsonConverters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public abstract class ConditionAndEffectAttribute(string kind) : Attribute
    {
        public string Kind { get; } = kind;
    }

    public sealed class SoundEffectKindAttribute(string kind) : ConditionAndEffectAttribute(kind)
    { }

    public sealed class SoundConditionKindAttribute(string kind) : ConditionAndEffectAttribute(kind)
    { }
}
