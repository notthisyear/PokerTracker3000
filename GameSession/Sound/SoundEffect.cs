using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using PokerTracker3000.Common;

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

        public int Id { get; } = ThreadSafeId.GetNext();
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
    public sealed class SynthesizedSpeechVariableAttribute(string name, string description) : Attribute
    {
        public string Name { get; } = name;

        public string Description { get; } = description;
    }

    public sealed class SpeechSoundEffect : MultipleOptionSoundEffect
    {
        public enum TextVariable
        {
            [SynthesizedSpeechVariable("$EVENT", "The game event name")]
            GameEvent,

            [SynthesizedSpeechVariable("$PNAME", "The name of the player")]
            PlayerName,

            [SynthesizedSpeechVariable("$PTOT", "The total of a player")]
            PlayerTotal,

            [SynthesizedSpeechVariable("$PBET", "The total of a player bet")]
            PlayerBet,

            [SynthesizedSpeechVariable("$TOTAL", "The total of the pot")]
            PotTotal,

            [SynthesizedSpeechVariable("$TIME", "The remaining stage time")]
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
