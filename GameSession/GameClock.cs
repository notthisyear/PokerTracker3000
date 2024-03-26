using System;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PokerTracker3000.GameSession
{
    public class GameClock : ObservableObject, IDisposable
    {
        #region Public properties

        #region Backing fields
        private int _numberOfSeconds = 0;
        private bool _isRunning = true;
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

        public event EventHandler? ClockHitZeroEvent;

        #region Private field
        private readonly Timer _tickClockTimer;
        private const int TickPeriodMs = 100;
        private const long TicksPerMillisecond = 10000;
        private const long MillisecondsPerSecond = 1000;
        private long _ticksOnLastFire = DateTime.MinValue.Ticks;
        private bool _pauseOnNextTick = false;
        private long _ticksUntilNextDecrease = MillisecondsPerSecond * TicksPerMillisecond;
        private bool _disposedValue;
        #endregion

        public GameClock()
        {
            _tickClockTimer = new(TickClock, default, Timeout.Infinite, Timeout.Infinite);
        }

        public void Start(int numberOfSeconds = -1)
        {
            if (numberOfSeconds > 0)
                NumberOfSeconds = numberOfSeconds;

            _ticksOnLastFire = DateTime.UtcNow.Ticks;
            _tickClockTimer.Change(TickPeriodMs, Timeout.Infinite);
            IsRunning = true;
        }

        public void Pause()
        {
            _pauseOnNextTick = true;
            IsRunning = false;
        }

        public void RegisterCallbackOnTimeLeft(int triggerPoint, Action callback)
        {
        }

        private void TickClock(object? state = default)
        {
            var ticksNow = DateTime.UtcNow.Ticks;
            var ticksDiff = ticksNow - _ticksOnLastFire;
            _ticksUntilNextDecrease -= ticksDiff;

            if (_ticksUntilNextDecrease <= 0)
            {
                _ticksUntilNextDecrease = MillisecondsPerSecond * TicksPerMillisecond;
                NumberOfSeconds--;
            }

            if (NumberOfSeconds == 0)
            {
                IsRunning = false;
            }
            else if (!_pauseOnNextTick)
            {
                _ticksOnLastFire = DateTime.UtcNow.Ticks;
                _tickClockTimer.Change(_ticksUntilNextDecrease < (TickPeriodMs * TicksPerMillisecond) ? (_ticksUntilNextDecrease / TicksPerMillisecond) : TickPeriodMs, Timeout.Infinite);
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
