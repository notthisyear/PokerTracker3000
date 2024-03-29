using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PokerTracker3000.GameSession
{
    public class GameClock : ObservableObject, IDisposable
    {
        #region Public properties

        #region Backing fields
        private int _numberOfSeconds = 0;
        private bool _isRunning = false;
        #endregion

        public int NumberOfSeconds
        {
            get => _numberOfSeconds;
            private set => SetProperty(ref _numberOfSeconds, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            private set => SetProperty(ref _isRunning, value);
        }
        #endregion

        #region Private field
        private const int TickPeriodMs = 100;
        private const long TicksPerMillisecond = 10000;
        private const long MillisecondsPerSecond = 1000;

        private readonly Timer _tickClockTimer;
        private readonly List<Action> _callbacksOnTick = [];
        private readonly Dictionary<int, List<Action<GameClock>>> _callbacksOnTimeLeft = [];

        private long _ticksOnLastFire = DateTime.MinValue.Ticks;
        private bool _pauseOnNextTick = false;
        private long _ticksUntilNextDecrease = MillisecondsPerSecond * TicksPerMillisecond;
        private bool _disposedValue;
        #endregion

        public GameClock()
        {
            _tickClockTimer = new(TickClock, default, Timeout.Infinite, Timeout.Infinite);
        }

        public void UpdateNumberOfSeconds(int numberOfSeconds)
        {
            if (numberOfSeconds > 0)
                NumberOfSeconds = numberOfSeconds;
        }

        public void Start(int numberOfSeconds = -1)
        {
            UpdateNumberOfSeconds(numberOfSeconds);
            _ticksOnLastFire = DateTime.UtcNow.Ticks;
            _tickClockTimer.Change(TickPeriodMs, Timeout.Infinite);
            IsRunning = true;
        }

        public void Pause()
        {
            _pauseOnNextTick = true;
            IsRunning = false;
        }

        public void Stop()
        {
            _tickClockTimer.Change(Timeout.Infinite, Timeout.Infinite);
            NumberOfSeconds = 0;
        }
        public void RegisterCallbackOnTimeLeft(int trigger, Action<GameClock> action)
        {
            if (!_callbacksOnTimeLeft.TryGetValue(trigger, out var actions))
                _callbacksOnTimeLeft.Add(trigger, []);
            _callbacksOnTimeLeft[trigger].Add(action);
        }

        public void RegisterCallbackOnTick(Action action)
        {
            _callbacksOnTick.Add(action);
        }
        private void TickClock(object? state = default)
        {
            var ticksNow = DateTime.UtcNow.Ticks;
            var ticksDiff = ticksNow - _ticksOnLastFire;
            _ticksUntilNextDecrease -= ticksDiff;

            if (_ticksUntilNextDecrease <= 0)
            {
                _ticksUntilNextDecrease = (MillisecondsPerSecond * TicksPerMillisecond) + _ticksUntilNextDecrease;
                NumberOfSeconds--;
                if (_callbacksOnTick.Count > 0)
                {
                    Task.Run(() =>
                    {
                        foreach (var action in _callbacksOnTick)
                            action.Invoke();
                    });
                }
                if (_callbacksOnTimeLeft.TryGetValue(NumberOfSeconds, out var value))
                {
                    Task.Run(() =>
                    {
                        foreach (var action in value)
                            action.Invoke(this);
                    });
                }
            }

            if (!_pauseOnNextTick)
            {
                _ticksOnLastFire = DateTime.UtcNow.Ticks;
                _tickClockTimer.Change(Math.Min(1, _ticksUntilNextDecrease < (TickPeriodMs * TicksPerMillisecond) ? (_ticksUntilNextDecrease / TicksPerMillisecond) : TickPeriodMs), Timeout.Infinite);
            }
            else
            {
                IsRunning = false;
                _pauseOnNextTick = false;
            }
        }

        #region Disposal
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _tickClockTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _tickClockTimer.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
