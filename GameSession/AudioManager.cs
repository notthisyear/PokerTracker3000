using System;
using System.Collections.Concurrent;
using System.Media;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using PokerTracker3000.Common;
using PokerTracker3000.Common.Messages;
using PokerTracker3000.GameComponents;
using PokerTracker3000.Interfaces;

namespace PokerTracker3000.GameSession
{
    public class AudioManager
    {
        private enum SoundType
        {
            Riff,
            Speech
        };

        private record SoundItem(SoundType Type, string Speech = "");

        #region Private fields
        private readonly IGameEventBus _eventBus;
        private readonly SpeechSynthesizer _synth;
        private readonly SoundPlayer _riffSoundPlayer;
        private readonly Thread _audioThread;
        private readonly ConcurrentQueue<SoundItem> _audioQueue;
        private readonly TaskCompletionSource _tcs = new();
        private bool _shouldExit = false;
        #endregion

        public AudioManager(IGameEventBus eventBus, GameClock clock, string pathToRiffSound)
        {
            _eventBus = eventBus;
            _synth = new SpeechSynthesizer();
            _riffSoundPlayer = new()
            {
                SoundLocation = pathToRiffSound
            };
            _synth.SetOutputToDefaultAudioDevice();

            _audioQueue = new ConcurrentQueue<SoundItem>();

            _audioThread = new(MonitorSpeechQueue) { IsBackground = true };
            _audioThread.Start();

            clock.RegisterCallbackOnSecondsLeft(63, (_) =>
            {
                _audioQueue.Enqueue(new(SoundType.Riff));
                _audioQueue.Enqueue(new(SoundType.Speech, "Attention players! This stage will end in one minute"));
            });

            _eventBus.RegisterListener(this, (t, m) => PlayerEventReceived(m),
                [GameEventBus.EventType.PlayerEliminated, GameEventBus.EventType.PlayerAddOn, GameEventBus.EventType.PlayerBuyIn]);
            _eventBus.RegisterListener(this, GameEventReceived,
                [GameEventBus.EventType.GameStarted, GameEventBus.EventType.GamePaused, GameEventBus.EventType.GameDone]);
            _eventBus.RegisterListener(this, (t, m) => StageEventReceived(m), GameEventBus.EventType.StageChanged);
            _eventBus.RegisterListener(this, (t, m) => ApplicationClosing(m), GameEventBus.EventType.ApplicationClosing);
        }

        #region Private fields
        private void MonitorSpeechQueue(object? obj)
        {
            _riffSoundPlayer.Load();
            while (!_shouldExit)
            {
                if (_audioQueue.TryDequeue(out var item))
                {
                    if (item.Type == SoundType.Riff)
                        _riffSoundPlayer.PlaySync();
                    else if (item.Type == SoundType.Speech)
                        _synth.Speak(item.Speech);
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }

            _riffSoundPlayer.Dispose();
            _synth.Dispose();
            _tcs.SetResult();
        }

        private void PlayerEventReceived(IInternalMessage message)
        {
            if (message is not PlayerEventMessage msg)
                return;

            var isFirstBetFromPlayer = (msg.MessageType == PlayerEventMessage.Type.AddOn
                || msg.MessageType == PlayerEventMessage.Type.BuyIn)
                && (msg.PlayerTotal - msg.AddOnOrBuyInAmount == 0);

            var (attr, _) = msg.Currency.GetCustomAttributeFromEnum<CurrencyAttribute>();
            var speechMsg = msg.MessageType switch
            {
                PlayerEventMessage.Type.AddOn => isFirstBetFromPlayer ?
                    $"{msg.PlayerName} joined the game with a {GetFormattedDecimal(msg.AddOnOrBuyInAmount)} {attr!.Code} bet. Welcome!" :
                    $"{msg.PlayerName} added another {GetFormattedDecimal(msg.AddOnOrBuyInAmount)} {attr!.Code} to the pot. The pot total is now {GetFormattedDecimal(msg.PotTotal)} {attr!.Code}. Nice.",
                PlayerEventMessage.Type.BuyIn => isFirstBetFromPlayer ?
                    $"{msg.PlayerName} joined the game with a {msg.AddOnOrBuyInAmount} {attr!.Code} bet. Welcome!" :
                    $"Chips please! {msg.PlayerName} bought in and added {GetFormattedDecimal(msg.AddOnOrBuyInAmount)} {attr!.Code} to the pot. The total is now {GetFormattedDecimal(msg.PotTotal)} {attr!.Code}.",
                PlayerEventMessage.Type.Eliminated => msg.PlayerTotal > 0 ?
                    $"{msg.PlayerName} is eliminated. That's {GetFormattedDecimal(msg.PlayerTotal)} {attr!.Code} you won't see again." : string.Empty,
                _ => default
            };

            if (!string.IsNullOrEmpty(speechMsg))
                _audioQueue.Enqueue(new SoundItem(SoundType.Speech, speechMsg));
        }

        private void GameEventReceived(GameEventBus.EventType type, IInternalMessage message)
        {
            if (message is not GameEventMessage)
                return;

            var msg = type switch
            {
                GameEventBus.EventType.GameStarted => "The game has now started. Good luck to all players.",
                GameEventBus.EventType.GamePaused => $"The game has been paused. {GetPauseComment()}",
                GameEventBus.EventType.GameDone => "All stages done, well done players. Goodnight, I'm out",
                _ => default
            };

            if (!string.IsNullOrEmpty(msg))
            {
                if (type == GameEventBus.EventType.GameStarted || type == GameEventBus.EventType.GameDone)
                    _audioQueue.Enqueue(new SoundItem(SoundType.Riff));
                _audioQueue.Enqueue(new SoundItem(SoundType.Speech, msg));
            }
        }

        private void StageEventReceived(IInternalMessage message)
        {
            if (message is not StageChangedMessage msg)
                return;

            var stageNumberOfMinutes = msg.StageNumberOfSeconds / 60;
            var useMinutes = stageNumberOfMinutes > 0;
            _audioQueue.Enqueue(new SoundItem(SoundType.Riff));
            _audioQueue.Enqueue(new SoundItem(SoundType.Speech, msg.IsPause ?
                $"Attention players, it's time for a break. Let's go get a new beer or have a smoke. " +
                $"See you in {GetNumberString(useMinutes ? stageNumberOfMinutes : msg.StageNumberOfSeconds, useMinutes ? "minute" : "second")}." :
                $"Attention players, we're now at stage {msg.StageNumber} and the blinds have changed. The small blind is {GetFormattedDecimal(msg.SmallBlind)} and the big blind is {GetFormattedDecimal(msg.BigBlind)}."));
        }

        private static string GetNumberString(int number, string unit)
            => $"{number} {unit}{(number > 1 ? "s" : "")}";

        private static string GetFormattedDecimal(decimal value)
            => decimal.IsInteger(value) ? $"{value:F0}" : $"{value:F2}";

        private static string GetPauseComment()
        {
            string[] options =
            {
                "Finally, I need to pee.",
                string.Empty,
                "See you in a bit.",
                string.Empty,
                "Don't forget to turn me back on again.",
                string.Empty,
                "Is it snack time?"
            };
            var r = new Random();
            var idx = r.Next(0, options.Length);
            return options[idx];
        }


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
