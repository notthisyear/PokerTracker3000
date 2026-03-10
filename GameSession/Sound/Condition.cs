using System;
using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using PokerTracker3000.Common;

namespace PokerTracker3000.GameSession.Sound
{
    public enum ConditionVariable
    {
        None,

        [Description("Game event")]
        GameEvent,

        [Description("Player name")]
        PlayerName,

        [Description("Player total")]
        PlayerTotal,

        [Description("Player bet")]
        PlayerBet,

        [Description("Pot total")]
        PotTotal,

        [Description("Stage time remaining")]
        StageTimeRemaining,
    }

    public enum ConditionCheckType
    {
        None,

        [Description("<")]
        LessThan,

        [Description("<=")]
        LessThanOrEqual,

        [Description("=")]
        Equal,

        [Description(">=")]
        GreaterThanOrEqual,

        [Description(">")]
        GreaterThan,

        [Description("!=")]
        NotEqual
    }

    public abstract class Condition : SelectableEntity
    {
        public ConditionVariable ConditionVariable { get; protected set; } = ConditionVariable.None;

        public ConditionCheckType ConditionCheckType { get; protected set; } = ConditionCheckType.None;

        public string ConditionValue { get; protected set; } = string.Empty;

        public string DisplayString { get; protected set; } = string.Empty;

        private const NumberStyles IntegerNumberStyle = NumberStyles.Integer;
        private const NumberStyles FloatingpointNumberStyle = NumberStyles.Float;
        private static readonly IFormatProvider s_invariantCulture = CultureInfo.InvariantCulture;

        public static (bool isValid, string validationInfo) IsValid(ConditionVariable variable, ConditionCheckType check, string value)
        {
            if (variable != ConditionVariable.GameEvent && string.IsNullOrEmpty(value))
                return (false, "Value is empty");

            if (variable == ConditionVariable.PlayerName)
            {
                if ((check != ConditionCheckType.Equal) && (check != ConditionCheckType.NotEqual))
                {
                    return ((check == ConditionCheckType.Equal) || (check == ConditionCheckType.NotEqual),
                    $"Player name check must be '{GetCheckTypeString(ConditionCheckType.Equal)}' or " +
                    $"'{GetCheckTypeString(ConditionCheckType.NotEqual)}'");
                }
            }
            else if (variable == ConditionVariable.StageTimeRemaining)
            {
                if (check != ConditionCheckType.Equal)
                    return (false, $"Stage time check must be '{GetCheckTypeString(ConditionCheckType.Equal)}'");
                else if (!int.TryParse(value, IntegerNumberStyle, s_invariantCulture, out _))
                    return (false, "Stage time must be valid integer");

            }
            else if (variable == ConditionVariable.GameEvent)
            {
                if (check != ConditionCheckType.Equal)
                    return (false, $"Game event check must be '{GetCheckTypeString(ConditionCheckType.Equal)}'");
            }
            else
            {
                if (!float.TryParse(value, FloatingpointNumberStyle, s_invariantCulture, out _))
                    return (false, "Value must be valid number");
            }

            return (true, string.Empty);
        }

        public static bool TryGet(ConditionVariable variable, ConditionCheckType checkType, string value, GameEventBus.EventType gameEvent, out Condition? condition)
        {
            condition = default;
            var (isValid, _) = IsValid(variable, checkType, value);
            if (!isValid)
                return false;

            condition = variable switch
            {
                ConditionVariable.GameEvent => new EventCondition(gameEvent),
                ConditionVariable.StageTimeRemaining => new TimeCondition(int.Parse(value, IntegerNumberStyle, s_invariantCulture)),
                ConditionVariable.PlayerName => new StringCondition(variable, checkType, value),
                ConditionVariable.PlayerTotal or
                ConditionVariable.PlayerBet or
                ConditionVariable.PotTotal => new NumberCondition<float>(variable, checkType, float.Parse(value, FloatingpointNumberStyle, s_invariantCulture)),
                _ => default,
            };

            return condition != default;
        }

        protected static string GetConditionVariableString(ConditionVariable checkType)
        {
            var (attr, e) = checkType.GetCustomAttributeFromEnum<DescriptionAttribute>();
            if (e != default)
                throw e;
            return attr!.Description;
        }

        protected static string GetCheckTypeString(ConditionCheckType checkType)
        {
            var (attr, e) = checkType.GetCustomAttributeFromEnum<DescriptionAttribute>();
            if (e != default)
                throw e;
            return attr!.Description;
        }
    }

    public abstract class Condition<T> : Condition
    {
        public abstract bool Check(T value);
    }

    public sealed class EventCondition : Condition<GameEventBus.EventType>
    {
        public GameEventBus.EventType Value { get; }

        public EventCondition(GameEventBus.EventType value)
        {
            Value = value;
            ConditionVariable = ConditionVariable.GameEvent;
            ConditionCheckType = ConditionCheckType.Equal;

            DisplayString = $"{GetConditionVariableString(ConditionVariable)} = {Value}";
        }

        public override bool Check(GameEventBus.EventType v)
            => Value == v;
    }

    public sealed class TimeCondition : Condition<int>
    {
        private readonly int _value;

        public TimeCondition(int value)
        {
            ConditionVariable = ConditionVariable.StageTimeRemaining;
            ConditionCheckType = ConditionCheckType.Equal;
            ConditionValue = value.ToString();
            _value = value;

            DisplayString = $"{GetConditionVariableString(ConditionVariable)} = {_value} s";
        }

        public override bool Check(int v)
            => _value == v;
    }

    public sealed class StringCondition : Condition<string>
    {
        private readonly Predicate<string> _check;

        public StringCondition(ConditionVariable variable, ConditionCheckType checkType, string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value), "StringCondition value cannot be empty");

            if (checkType != ConditionCheckType.Equal && checkType != ConditionCheckType.NotEqual)
                throw new ArgumentException("StringCondition can only check equality", nameof(checkType));

            ConditionVariable = variable;
            ConditionCheckType = checkType;
            ConditionValue = value;
            _check = checkType == ConditionCheckType.Equal ?
                (v => string.Equals(ConditionValue, v, StringComparison.Ordinal)) :
                (v => !string.Equals(ConditionValue, v, StringComparison.Ordinal));
            DisplayString = $"{GetConditionVariableString(ConditionVariable)} {GetCheckTypeString(ConditionCheckType)} \"{ConditionValue}\"";
        }

        public override bool Check(string v)
            => _check(v);
    }

    public sealed class NumberCondition<T> : Condition<T> where T : INumber<T>
    {
        private readonly T _value;
        private readonly Predicate<T> _check;

        public NumberCondition(ConditionVariable variable, ConditionCheckType checkType, T value)
        {
            ConditionVariable = variable;
            ConditionCheckType = checkType;
            ConditionValue = value.ToString() ?? string.Empty;

            _value = value;
            _check = checkType switch
            {
                ConditionCheckType.LessThan => v => _value < v,
                ConditionCheckType.LessThanOrEqual => v => _value <= v,
                ConditionCheckType.Equal => v => _value == v,
                ConditionCheckType.GreaterThanOrEqual => v => _value >= v,
                ConditionCheckType.GreaterThan => v => _value > v,
                ConditionCheckType.NotEqual => v => _value != v,
                _ => throw new NotImplementedException(),
            };
            DisplayString = $"{GetConditionVariableString(ConditionVariable)} {GetCheckTypeString(ConditionCheckType)} {_value}";
        }

        public override bool Check(T v)
            => _check(v);
    }
}
