using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using PokerTracker3000.Common;
using PokerTracker3000.Interfaces;

namespace PokerTracker3000.GameSession.Sound
{
    public sealed class SoundEffectOption : SelectableEntity
    {
        #region Backing fields
        private string _name = string.Empty;
        private string _path = string.Empty;
        #endregion

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public string Path
        {
            get { return _path; }
            set
            {
                Name = System.IO.Path.GetFileName(value);
                SetProperty(ref _path, value);
            }
        }

        private readonly int _id = ThreadSafeId.GetNext();

        public int Id => _id;
    }

    public abstract class SoundEffect : SelectableEntity
    {
        #region Public properties

        #region Backing fields
        private string _name = string.Empty;
        #endregion

        public string Name
        {
            get { return _name; }
            protected set { SetProperty(ref _name, value); }
        }

        public abstract SoundEffectType Type { get; }
        #endregion
    };

    public abstract class MultipleOptionSoundEffect : SoundEffect
    {
        #region Backing fields
        private MultipleOptionMode _mode = MultipleOptionMode.None;
        private string _rawName = string.Empty;
        #endregion

        public MultipleOptionMode Mode
        {
            get => _mode;
            set
            {
                SetProperty(ref _mode, value);
                SetName();
            }
        }
        public string RawName
        {
            get => _rawName;
            set
            {
                SetProperty(ref _rawName, value);
                SetName();
            }
        }

        public virtual ObservableCollection<SoundEffectOption> EffectOptions { get; } = [];


        #region Private properties
        private int _lastOptionReturnedIndex = 0;
        #endregion

        #region Public & protected methods
        public virtual int GetNextOptionIndex()
        {
            return 0;
        }

        protected int GetNextOptionIndexImpl(int maxIndex)
        {
            switch (Mode)
            {
                case MultipleOptionMode.Sequential:
                    var v = _lastOptionReturnedIndex % maxIndex;
                    _lastOptionReturnedIndex = (_lastOptionReturnedIndex + 1) % (maxIndex);
                    return v;

                case MultipleOptionMode.Random:
                    return Random.Shared.Next(maxIndex);

                case MultipleOptionMode.Shuffle:
                    // TODO: Implement
                    return 0;
                case MultipleOptionMode.None:
                default:
                    return 0;
            }
        }

        public void SetName()
        {
            Name = $"{RawName} ({Mode}, {EffectOptions.Count} opts.)";
        }
        #endregion
    }

    public sealed class BuiltInSoundEffect : SoundEffect
    {
        #region Public properties
        public BuiltInSoundEffectType BuiltInEffect { get; } = BuiltInSoundEffectType.None;

        public override SoundEffectType Type => SoundEffectType.BuiltIn;
        #endregion

        public BuiltInSoundEffect(BuiltInSoundEffectType builtInEffect)
        {
            BuiltInEffect = builtInEffect;
            var (attr, _) = builtInEffect.GetCustomAttributeFromEnum<DescriptionAttribute>();
            Name = $"{attr!.Description} (Built-in)";
        }
    };

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SynthesizedSpeechVariableAttribute(string name, string description, string defaultReplacement) : Attribute
    {
        public string Name { get; } = name;

        public string Description { get; } = description;

        public string DefaultReplacement { get; } = defaultReplacement;

        public bool HasVariable(string message)
            => message.Contains(Name);

        public string Replace(string message, string? replacement = default)
            => message.Replace(Name, string.IsNullOrEmpty(replacement) ? DefaultReplacement : replacement);

    }

    public sealed class SpeechSoundEffect : MultipleOptionSoundEffect
    {
        public enum TextVariable
        {
            [SynthesizedSpeechVariable("$EVENT", "The game event name", "Event")]
            GameEvent,

            [SynthesizedSpeechVariable("$PNAME", "The name of the player", "Player")]
            PlayerName,

            [SynthesizedSpeechVariable("$PTOT", "The total of a player", "Player total")]
            PlayerTotal,

            [SynthesizedSpeechVariable("$PBET", "The total of a player bet", "Player bet")]
            PlayerBet,

            [SynthesizedSpeechVariable("$TOTAL", "The total of the pot", "Pot total")]
            PotTotal,

            [SynthesizedSpeechVariable("$TIME", "The remaining stage time", "Time")]
            StageTimeRemaning,
        }

        public override SoundEffectType Type => SoundEffectType.Speech;

        public override ObservableCollection<SoundEffectOption> EffectOptions { get; }

        #region Private fields
        private readonly object _lock = new();
        #endregion

        public SpeechSoundEffect(string name)
        {
            EffectOptions = [];
            BindingOperations.EnableCollectionSynchronization(EffectOptions, _lock);
            RawName = name;
        }

        public override int GetNextOptionIndex()
            => GetNextOptionIndexImpl(EffectOptions.Count);

        public void AddSpeechOption(string speechOption)
        {
            lock (_lock)
            {
                EffectOptions.Add(new() { Name = speechOption });
            }
            SetName();
        }

        public void RemoveSpeechOption(SoundEffectOption option)
        {
            lock (_lock)
            {
                var match = EffectOptions.FirstOrDefault(x => x.Id == option.Id);
                if (match != default)
                    EffectOptions.Remove(match);
            }
            SetName();
        }

        public static string GetSpeechForSoundEffectOption(SoundEffectOption option, IInternalMessage? message = default)
        {
            var result = option.Name;
            foreach (var textVariable in Enum.GetValues<TextVariable>())
            {
                if (HasVariable(textVariable, result))
                    result = s_variableAttributeLookup[textVariable].Replace(result, GetVariableValueFromMessage(message));
            }
            return result;
        }

        #region Private fields and methods
        private static readonly Dictionary<TextVariable, SynthesizedSpeechVariableAttribute> s_variableAttributeLookup = [];

        private static bool HasVariable(TextVariable variable, string message)
        {
            lock (s_variableAttributeLookup)
            {
                if (s_variableAttributeLookup.Count == 0)
                {
                    foreach (var textVariable in Enum.GetValues<TextVariable>())
                        s_variableAttributeLookup.Add(textVariable, textVariable.GetCustomAttributeFromEnum<SynthesizedSpeechVariableAttribute>().attr!);
                }
            }

            return s_variableAttributeLookup[variable].HasVariable(message);
        }

        private static string GetVariableValueFromMessage(IInternalMessage? message)
        {
            if (message == default)
                return string.Empty;

            // TODO: Implement
            return message switch
            {
                _ => string.Empty,
            };
        }
        #endregion
    }

    public sealed class FromFileSoundEffect : MultipleOptionSoundEffect
    {
        public override SoundEffectType Type => SoundEffectType.File;

        public override ObservableCollection<SoundEffectOption> EffectOptions { get; }

        #region Private fields
        private readonly object _lock = new();
        #endregion

        public FromFileSoundEffect(string name)
        {
            EffectOptions = [];
            BindingOperations.EnableCollectionSynchronization(EffectOptions, _lock);
            RawName = name;
        }

        public void AddFileOption(string path)
        {
            lock (_lock)
            {
                EffectOptions.Add(new() { Path = path });
            }
            SetName();
        }

        public void RemoveFileOption(SoundEffectOption option)
        {
            lock (_lock)
            {
                var match = EffectOptions.FirstOrDefault(x => x.Id == option.Id);
                if (match != default)
                    EffectOptions.Remove(match);
            }
            SetName();
        }

        public override int GetNextOptionIndex()
            => GetNextOptionIndexImpl(EffectOptions.Count);
    };
}
