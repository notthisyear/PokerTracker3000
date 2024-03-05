using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PokerTracker3000.GameSession
{
    public class GameClock : ObservableObject, IDisposable
    {
        #region Public properties

        #region Backing fields
        private int _numberOfSeconds = 20 * 60;
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
            set
            {
                if (_isRunning == value)
                    return;

                if (!value)
                {
                    _tickClockTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _timeLeftWhenTimerPausedMs = _ticksOnLastFire == DateTime.MinValue.Ticks ? 1000 : (int)((_ticksOnLastFire - DateTime.UtcNow.Ticks) / 10000);
                }
                else
                {
                    _tickClockTimer.Change(_timeLeftWhenTimerPausedMs, Timeout.Infinite);
                }
                _isRunning = value;
            }
        }
        #endregion

        public event EventHandler? ClockHitZeroEvent;

        #region Private field
        private readonly Timer _tickClockTimer;
        private int _timeLeftWhenTimerPausedMs = 1000;
        private long _ticksOnLastFire = DateTime.MinValue.Ticks;
        private bool _disposedValue;
        #endregion

        public GameClock()
        {
            _tickClockTimer = new(TickClock, default, 1000, Timeout.Infinite);        
        }

        private void TickClock(object? state = default)
        {
            NumberOfSeconds--;
            _ticksOnLastFire = DateTime.UtcNow.Ticks;
            if (NumberOfSeconds == 0)
            {
                _ticksOnLastFire = DateTime.MinValue.Ticks;
                IsRunning = false;
                Task.Run(() => ClockHitZeroEvent?.Invoke(this, EventArgs.Empty));
            }
            else
            {
                _tickClockTimer.Change(1000, Timeout.Infinite);
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
