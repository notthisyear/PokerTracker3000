using System.Collections.Concurrent;
using System.Media;
using System.Speech.Synthesis;
using System.Threading;
using PokerTracker3000.GameSession;

namespace PokerTracker3000.Common
{
    public class AudioManager
    {
        private enum SoundType
        {
            Riff,
            Speech
        };

        private record SoundItem(SoundType Type, string Speech = "");

        private readonly SpeechSynthesizer _synth;
        private readonly SoundPlayer _riffSoundPlayer;
        private readonly Thread _audioThread;
        private readonly ConcurrentQueue<SoundItem> _audioQueue;

        public AudioManager(string pathToRiffSound, GameClock clock)
        {
            _synth = new SpeechSynthesizer();
            _riffSoundPlayer = new()
            {
                SoundLocation = pathToRiffSound
            };
            _synth.SetOutputToDefaultAudioDevice();

            _audioQueue = new ConcurrentQueue<SoundItem>();

            _audioThread = new(MonitorSpeechQueue) { IsBackground = true };
            _audioThread.Start();

            clock.RegisterCallbackOnSecondsLeft(60, (_) =>
            {
                _audioQueue.Enqueue(new(SoundType.Riff));
                _audioQueue.Enqueue(new(SoundType.Speech, "Attention players! This stage will end in one minute"));
            });

        }

        private void MonitorSpeechQueue(object? obj)
        {
            _riffSoundPlayer.Load();
            while (true)
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
        }
    }
}
