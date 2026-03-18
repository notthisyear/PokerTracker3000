using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using PokerTracker3000.Common;
using PokerTracker3000.Common.Messages;
using PokerTracker3000.Interfaces;

namespace PokerTracker3000.GameSession.Sound
{
    public class AudioManager
    {
        public ObservableCollection<SoundEvent> GameSounds { get; } = [];

        private record SoundItem(SoundEffect Effect, IInternalMessage? Message, TaskCompletionSource? Tcs, int OptionId)
        {
            public static SoundItem Get(SoundEffect effect)
                => new(effect, default, default, -1);

            public static SoundItem Get(SoundEffect effect, IInternalMessage message)
                => new(effect, message, default, -1);

            public static SoundItem Get(SoundEffect effect, TaskCompletionSource? tcs, int optionId)
                => new(effect, default, tcs, optionId);

            public static SoundItem Get(SoundEffect effect, IInternalMessage message, TaskCompletionSource tcs, int optionId)
                => new(effect, message, tcs, optionId);
        }

        #region Private fields
        private readonly IGameEventBus _eventBus;
        private readonly SpeechSynthesizer _synth;
        private readonly Thread _audioThread;
        private readonly ConcurrentQueue<SoundItem> _audioQueue;
        private readonly Dictionary<BuiltInSoundEffectType, string> _builtInSoundEffectsPaths;
        private readonly TaskCompletionSource _tcs = new();
        private readonly object _gameSoundsAccessLock = new();

        private bool _shouldExit = false;
        #endregion

        public AudioManager(IGameEventBus eventBus, Dictionary<BuiltInSoundEffectType, string> builtInEffectPaths)
        {
            _eventBus = eventBus;
            _synth = new SpeechSynthesizer();
            _synth.SetOutputToDefaultAudioDevice();

            _builtInSoundEffectsPaths = builtInEffectPaths;

            BindingOperations.EnableCollectionSynchronization(GameSounds, _gameSoundsAccessLock);

            _audioQueue = new();

            _audioThread = new(_ => MonitorAudioQueue()) { IsBackground = true };
            _audioThread.Start();

            //_eventBus.RegisterListener(this, AudioTriggeringEventReceived,
            //    [
            //        GameEventBus.EventType.TimeEvent,
            //        GameEventBus.EventType.PlayerEliminated,
            //        GameEventBus.EventType.PlayerAddOn,
            //        GameEventBus.EventType.PlayerBuyIn,
            //        GameEventBus.EventType.NewChipLead,
            //        GameEventBus.EventType.GameStarted,
            //        GameEventBus.EventType.GamePaused,
            //        GameEventBus.EventType.GameDone,
            //        GameEventBus.EventType.StageChanged
            //    ]);
            //_eventBus.RegisterListener(this, (_, m) => TimeLeftEventReceived(m), GameEventBus.EventType.TimeEvent);
            //_eventBus.RegisterListener(this, (t, m) => PlayerEventReceived(t, m),
            //    [
            //        GameEventBus.EventType.PlayerEliminated,
            //        GameEventBus.EventType.PlayerAddOn,
            //        GameEventBus.EventType.PlayerBuyIn,
            //        GameEventBus.EventType.NewChipLead
            //    ]);
            //_eventBus.RegisterListener(this, GameEventReceived,
            //    [
            //        GameEventBus.EventType.GameStarted,
            //        GameEventBus.EventType.GamePaused,
            //        GameEventBus.EventType.GameDone
            //    ]);
            //_eventBus.RegisterListener(this, (t, m) => StageEventReceived(m), GameEventBus.EventType.StageChanged);
            _eventBus.RegisterListener(this, (t, m) => ApplicationClosing(m), GameEventBus.EventType.ApplicationClosing);
        }

        #region Public methods
        public void TestSoundEffect(SoundEffect effect, TaskCompletionSource? tcs = default, int optionId = -1)
        {
            _audioQueue.Enqueue(SoundItem.Get(effect, tcs, optionId));
        }

        public void AddSoundEvent(SoundEvent soundEvent)
        {
            lock (_gameSoundsAccessLock)
            {
                GameSounds.Add(soundEvent);
            }
        }

        public void RemoveSoundEvent(int soundEventId)
        {
            lock (_gameSoundsAccessLock)
            {
                var matchingSound = GameSounds.FirstOrDefault(x => x.Id == soundEventId);
                if (matchingSound != default)
                    GameSounds.Remove(matchingSound);
            }
        }

        //public void AddEventSound(GameEventBus.EventType eventType, params SoundEffect[] effects)
        //{
        //    lock (_gameSoundsAccessLock)
        //    {
        //        var matchingEvent = GameSounds.FirstOrDefault(x => x.EventType == eventType);
        //        if (matchingEvent == default)
        //        {
        //            GameSounds.Add(new(eventType, effects));
        //        }
        //        else
        //        {
        //            foreach (var effect in effects)
        //                matchingEvent.AddSoundEffectToEvent(effect);
        //        }
        //    }
        //}
        #endregion

        #region Private methods
        //private void AudioTriggeringEventReceived(GameEventBus.EventType eventType, IInternalMessage message)
        //{
        //    lock (_gameSoundsAccessLock)
        //    {
        //        var matchingEvent = GameSounds.FirstOrDefault(x => x.EventType == eventType);
        //        if (matchingEvent == default)
        //        {
        //            return;
        //        }

        //        foreach (var effect in matchingEvent.SoundEffectsOnEvent)
        //        {
        //            if (effect.HasCondition)
        //            {
        //                if (!effect.Condition.Invoke(message))
        //                    continue;
        //            }
        //            _audioQueue.Enqueue((effect, message));
        //        }
        //    }
        //}

        private void MonitorAudioQueue()
        {
            while (!_shouldExit)
            {
                if (_audioQueue.TryDequeue(out var item))
                {
                    switch (item.Effect)
                    {
                        case BuiltInSoundEffect builtInEffect:
                            if (_builtInSoundEffectsPaths.TryGetValue(builtInEffect.BuiltInEffect, out var audioPath))
                            {
                                using var player = new SoundPlayer(audioPath);
                                player.PlaySync();
                                item.Tcs?.SetResult();
                            }
                            break;

                        case FromFileSoundEffect fromFileSoundEffect:
                            {
                                var option = (item.OptionId > -1) ?
                                    fromFileSoundEffect.EffectOptions.FirstOrDefault(x => x.Id == item.OptionId) :
                                    fromFileSoundEffect.EffectOptions[fromFileSoundEffect.GetNextOptionIndex()];

                                if (option != default)
                                {
                                    using var player = new SoundPlayer(option.Path);
                                    player.PlaySync();
                                    item.Tcs?.SetResult();
                                }
                            }
                            break;

                        case SpeechSoundEffect speechSoundEffect:
                            {
                                var option = (item.OptionId > -1) ?
                                    speechSoundEffect.EffectOptions.FirstOrDefault(x => x.Id == item.OptionId) :
                                    speechSoundEffect.EffectOptions[speechSoundEffect.GetNextOptionIndex()];

                                if (option != default)
                                {
                                    _synth.Speak(SpeechSoundEffect.GetSpeechForSoundEffectOption(option, item.Message));
                                    item.Tcs?.SetResult();
                                }
                            }
                            break;
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }

            _synth.Dispose();
            _tcs.SetResult();
        }

        //private string GetPathForBuiltInEffect(BuiltInSoundEffectType effectType)
        //    => effectType switch
        //    {
        //        BuiltInSoundEffectType.Riff => _riffSoundEffectPath,
        //        _ => string.Empty
        //    };

        //private void TimeLeftEventReceived(IInternalMessage message)
        //{
        //    if (message is not TimeEventMessage msg)
        //        return;

        //    lock (_gameSoundsAccessLock)
        //    {
        //        var matchingEvent = GameSounds.FirstOrDefault(x => x.EventType == GameEventBus.EventType.TimeEvent);
        //        if (matchingEvent == default)
        //        {
        //            return;
        //        }

        //        foreach (var effect in matchingEvent.SoundEffectsOnEvent)
        //        {
        //            if (effect.HasCondition && effect is IConditionSoundEffect<int> conditionEffect)
        //            {
        //                if (!conditionEffect.Condition(msg.EventTimeSeconds))
        //                    continue;
        //            }
        //            _audioQueue.Enqueue(effect);
        //        }
        //    }
        //}

        //private void PlayerEventReceived(IInternalMessage message)
        //{
        //    if (message is not PlayerEventMessage msg)
        //        return;

        //    lock (_gameSoundsAccessLock)
        //    {
        //        var matchingEvent = GameSounds.FirstOrDefault(x => x.EventType == GameEventBus.EventType.);
        //        if (matchingEvent == default)
        //        {
        //            return;
        //        }
        //    }

        //    if (msg.MessageType == PlayerEventMessage.Type.NewChipLead)
        //    {
        //        _audioQueue.Enqueue(new SoundItem(SoundType.NewChipLead));
        //        return;
        //    }

        //    var isFirstBetFromPlayer = (msg.MessageType == PlayerEventMessage.Type.AddOn
        //        || msg.MessageType == PlayerEventMessage.Type.BuyIn)
        //        && (msg.PlayerTotal - msg.AddOnOrBuyInAmount == 0);

        //    var (attr, _) = msg.Currency.GetCustomAttributeFromEnum<CurrencyAttribute>();
        //    var (speechMsg, soundType) = msg.MessageType switch
        //    {
        //        PlayerEventMessage.Type.AddOn => (isFirstBetFromPlayer ?
        //            $"{msg.PlayerName} joined the game with a {GetFormattedDecimal(msg.AddOnOrBuyInAmount)} {attr!.Code} bet. Welcome!" :
        //            $"{msg.PlayerName} added another {GetFormattedDecimal(msg.AddOnOrBuyInAmount)} {attr!.Code} to the pot. The pot total is now {GetFormattedDecimal(msg.PotTotal)} {attr!.Code}. Nice.",
        //            SoundType.None),
        //        PlayerEventMessage.Type.BuyIn => (isFirstBetFromPlayer ?
        //            $"{msg.PlayerName} joined the game with a {msg.AddOnOrBuyInAmount} {attr!.Code} bet. Welcome!" :
        //            $"Chips please! {msg.PlayerName} bought in and added {GetFormattedDecimal(msg.AddOnOrBuyInAmount)} {attr!.Code} to the pot. The total is now {GetFormattedDecimal(msg.PotTotal)} {attr!.Code}.",
        //             SoundType.PlayerBuyIn),
        //        PlayerEventMessage.Type.Eliminated => (msg.PlayerTotal > 0 ?
        //            $"{msg.PlayerName} is eliminated. That's {GetFormattedDecimal(msg.PlayerTotal)} {attr!.Code} you won't see again." : string.Empty,
        //             SoundType.PlayerEliminated),
        //        _ => (string.Empty, SoundType.None)
        //    };

        //    if (!string.IsNullOrEmpty(speechMsg))
        //    {
        //        var useSpeech = isFirstBetFromPlayer; //  || Random.Shared.Next(1) == 1;
        //        if (useSpeech || soundType == SoundType.None)
        //            _audioQueue.Enqueue(new SoundItem(SoundType.Speech, speechMsg));
        //        else
        //            _audioQueue.Enqueue(new SoundItem(soundType));
        //    }
        //}

        //private void GameEventReceived(GameEventBus.EventType type, IInternalMessage message)
        //{
        //    if (message is not GameEventMessage)
        //        return;

        //    var msg = type switch
        //    {
        //        GameEventBus.EventType.GameStarted => "The game has now started. Good luck to all players.",
        //        GameEventBus.EventType.GamePaused => $"The game has been paused. {GetPauseComment()}",
        //        GameEventBus.EventType.GameDone => "All stages done, well done players. Goodnight, I'm out",
        //        _ => default
        //    };

        //    if (!string.IsNullOrEmpty(msg))
        //    {
        //        if (type == GameEventBus.EventType.GameStarted || type == GameEventBus.EventType.GameDone)
        //            _audioQueue.Enqueue(new SoundItem(SoundType.StageRiff));
        //        _audioQueue.Enqueue(new SoundItem(SoundType.Speech, msg));
        //    }
        //}

        //private void StageEventReceived(IInternalMessage message)
        //{
        //    if (message is not StageChangedMessage msg)
        //        return;

        //    var stageNumberOfMinutes = msg.StageNumberOfSeconds / 60;
        //    var useMinutes = stageNumberOfMinutes > 0;
        //    _audioQueue.Enqueue(new SoundItem(SoundType.StageRiff));
        //    _audioQueue.Enqueue(new SoundItem(SoundType.Speech, msg.IsPause ?
        //        $"Attention players, it's time for a break. Let's go get a new beer or have a smoke. " +
        //        $"See you in {GetNumberString(useMinutes ? stageNumberOfMinutes : msg.StageNumberOfSeconds, useMinutes ? "minute" : "second")}." :
        //        $"Attention players, we're now at stage {msg.StageNumber} and the blinds have changed. The small blind is {GetFormattedDecimal(msg.SmallBlind)} and the big blind is {GetFormattedDecimal(msg.BigBlind)}."));
        //}

        //private static string GetNumberString(int number, string unit)
        //    => $"{number} {unit}{(number > 1 ? "s" : "")}";

        //private static string GetFormattedDecimal(decimal value)
        //    => decimal.IsInteger(value) ? $"{value:F0}" : $"{value:F2}";

        //private static string GetPauseComment()
        //{
        //    string[] options =
        //    {
        //        "Finally, I need to pee.",
        //        string.Empty,
        //        "See you in a bit.",
        //        string.Empty,
        //        "Don't forget to turn me back on again.",
        //        string.Empty,
        //        "Is it snack time?"
        //    };
        //    var r = new Random();
        //    var idx = r.Next(0, options.Length);
        //    return options[idx];
        //}

        private void ApplicationClosing(IInternalMessage m)
        {
            if (m is not ApplicationClosingMessage message)
                return;

            _shouldExit = true;
            _tcs.Task.Wait();
            message.NumberOfClosingCallbacksCalled++;
        }
        #endregion
    }
}
