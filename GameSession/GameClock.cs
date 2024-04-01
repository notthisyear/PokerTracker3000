using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.Common;
using PokerTracker3000.Common.Messages;
using PokerTracker3000.Interfaces;

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

        private readonly IGameEventBus _eventBus;
        private readonly Timer _tickClockTimer;
        private readonly List<Action> _callbacksOnTick = [];
        private readonly Dictionary<int, List<Action<GameClock>>> _callbacksOnTimeLeft = [];

        private long _ticksOnLastFire = DateTime.MinValue.Ticks;
        private bool _pauseOnNextTick = false;
        private bool _isStopped = false;
        private long _ticksUntilNextDecrease = MillisecondsPerSecond * TicksPerMillisecond;
        private bool _disposedValue;
        #endregion

        public GameClock(IGameEventBus eventBus)
        {
            _eventBus = eventBus;
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
            _isStopped = false;
            _eventBus.NotifyListeners(GameEventBus.EventType.GameStarted, new GameEventMessage());
        }

        public void Pause()
        {
            IsRunning = false;
            if (!_isStopped)
            {
                _pauseOnNextTick = true;
                _eventBus.NotifyListeners(GameEventBus.EventType.GamePaused, new GameEventMessage());
            }
            _isStopped = false;
        }

        public void Stop()
        {
            NumberOfSeconds = 0;
            _isStopped = true;
            _tickClockTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void RegisterCallbackOnSecondsLeft(int triggerSeconds, Action<GameClock> action)
        {
            if (!_callbacksOnTimeLeft.TryGetValue(triggerSeconds, out var actions))
                _callbacksOnTimeLeft.Add(triggerSeconds, []);
            _callbacksOnTimeLeft[triggerSeconds].Add(action);
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
                _tickClockTimer.Change(Math.Max(1, _ticksUntilNextDecrease < (TickPeriodMs * TicksPerMillisecond) ? (_ticksUntilNextDecrease / TicksPerMillisecond) : TickPeriodMs), Timeout.Infinite);
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
