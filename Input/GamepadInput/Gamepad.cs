using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using PokerTracker3000.GamepadInput.XInput;
using PokerTracker3000.GamepadInput.XInput.Enumerations;
using PokerTracker3000.Input.GamepadInput;

namespace PokerTracker3000.GamepadInput
{
    public class Gamepad : IDisposable
    {
        #region Events
        public event EventHandler? GamepadDisconnected;
        public event EventHandler<InputEvent>? NewGamepadInput;
        public event EventHandler<GamepadBatteryLevel>? NewGamepadBatteryLevel;
        #endregion

        #region Public properties
        public uint Id { get; }

        public bool IsWireless { get; }

        public bool CanGetBatteryLevel { get; }
        #endregion

        #region Private fields
        private uint _lastPacketNumber = uint.MaxValue;
        private GamepadState? _lastGamepadState;
        private BatteryLevel _lastBatteryLevel = BatteryLevel.BATTERY_LEVEL_EMPTY;
        private readonly ConcurrentQueue<GamepadInternalEvent> _gamepadEvents;
        private readonly Timer _checkBatteryLevelTimer;
        private readonly Timer _checkControllerStateTimer;
        private readonly Thread _sendEventThread;

        private const int GamepadStateMonitorIntervalMs = 50;
        private const int CheckBatteryStateIntervalMs = 300000;
        private readonly object _disposalLock = new();
        private bool _disposedValue;

        private readonly DeadzoneSetting _leftStickDeadzone = new() { NegativeXPercent = 0.03f, PositiveXPercent = 0.03f, PositiveYPercent = 0.03f, NegativeYPercent = 0.03f };
        private readonly DeadzoneSetting _rightStickDeadzone = new() { NegativeXPercent = 0.03f, PositiveXPercent = 0.03f, PositiveYPercent = 0.03f, NegativeYPercent = 0.03f };
        #endregion

        private Gamepad(uint id, bool isWireless, bool canGetBatteryLevel)
        {
            Id = id;
            IsWireless = isWireless;
            CanGetBatteryLevel = canGetBatteryLevel;

            _gamepadEvents = new();
            _sendEventThread = new Thread(SendEventThread) { IsBackground = true };
            _sendEventThread.Start();

            _checkBatteryLevelTimer = new((s) => CheckBatteryLevel(), default, canGetBatteryLevel ? 1 : Timeout.Infinite, Timeout.Infinite);
            _checkControllerStateTimer = new((s) => CheckControllerState(), default, GamepadStateMonitorIntervalMs, Timeout.Infinite);
        }

        internal static bool TryCreate(uint id, out Gamepad? gamepad)
        {
            if (!GetAndSetControllerCapabilities(id, out var isWireless, out var canGetBatteryLevel))
            {
                gamepad = default;
                return false;
            }
            gamepad = new(id, isWireless, canGetBatteryLevel);
            return true;
        }

        #region Private methods
        private static bool GetAndSetControllerCapabilities(uint id, out bool isWireless, out bool canGetBatteryLevel)
        {
            isWireless = false;
            canGetBatteryLevel = false;
            XINPUT_CAPABILITIES capabilities = new();
            if (ImportedMethods.XInputGetCapabilities(id,
                XInputControllerTypeFlag.XINPUT_FLAG_GAMEPAD.ToUnsignedInteger(), ref capabilities).ToErrorCode() != Win32SystemErrorCodes.ERROR_SUCCESS)
            {
                return false;
            }

            if (capabilities.Type.ToControllerType() != XInputControllerType.XINPUT_DEVTYPE_GAMEPAD ||
                capabilities.SubType.ToControllerSubtype() != XInputControllerSubtype.XINPUT_DEVSUBTYPE_GAMEPAD)
            {
                return false;
            }

            isWireless = capabilities.Flags.GetControllerFeatureFlags().Where(x => x == XInputControllerFeatureFlag.XINPUT_CAPS_WIRELESS).Any();


            XINPUT_BATTERY_INFORMATION batteryInformation = new();
            if (ImportedMethods.XInputGetBatteryInformation(id,
                XInputBatteryDeviceType.BATTERY_DEVTYPE_GAMEPAD.ToUnsignedByte(), ref batteryInformation).ToErrorCode() == Win32SystemErrorCodes.ERROR_SUCCESS)
            {
                var batteryType = batteryInformation.BatteryType.ToBatteryType();
                canGetBatteryLevel = batteryType == BatteryType.BATTERY_TYPE_ALKALINE || batteryType == BatteryType.BATTERY_TYPE_NIMH;
            }

            return true;
        }

        private void CheckBatteryLevel()
        {
            var sendBatteryUpdate = false;
            XINPUT_BATTERY_INFORMATION batteryInformation = new();
            if (ImportedMethods.XInputGetBatteryInformation(Id, XInputBatteryDeviceType.BATTERY_DEVTYPE_GAMEPAD.ToUnsignedByte(), ref batteryInformation).ToErrorCode() == Win32SystemErrorCodes.ERROR_SUCCESS)
            {
                if (batteryInformation.BatteryLevel.ToBatteryLevel() != _lastBatteryLevel)
                {
                    _lastBatteryLevel = batteryInformation.BatteryLevel.ToBatteryLevel();
                    sendBatteryUpdate = true;
                }
            }

            if (sendBatteryUpdate)
                _gamepadEvents.Enqueue(GamepadInternalEvent.CreateBatteryLevelEvent(Id, _lastBatteryLevel));

            lock (_disposalLock)
            {
                if (!_disposedValue)
                    _checkBatteryLevelTimer.Change(CheckBatteryStateIntervalMs, Timeout.Infinite);
            }
        }

        private void CheckControllerState()
        {
            var t = Environment.TickCount;
            XINPUT_STATE state = new();
            if (ImportedMethods.XInputGetState(Id, ref state).ToErrorCode() == Win32SystemErrorCodes.ERROR_SUCCESS)
            {
                if (state.dwPacketNumber != _lastPacketNumber)
                {
                    _lastPacketNumber = state.dwPacketNumber;
                    var newState = GamepadState.Create(Id, state.Gamepad);
                    if (_lastGamepadState == default)
                        _gamepadEvents.Enqueue(GamepadInternalEvent.CreateInputEvent(Id));
                    else
                        _gamepadEvents.Enqueue(GamepadInternalEvent.CreateInputEvent(Id, _lastGamepadState, newState, _leftStickDeadzone, _rightStickDeadzone));
                    _lastGamepadState = newState;
                }

                // TODO: Investigate the performance of this approach compared to a
                //       try-catch on the ObjectDisposedException. As there's normally
                //       no contention on this lock, it "should" be quite cheap, but
                //       measuring it would be the only way to know for sure.
                lock (_disposalLock)
                {
                    if (!_disposedValue)
                    {
                        var tDiff = Environment.TickCount - t;
                        _checkControllerStateTimer.Change(Math.Max(1, GamepadStateMonitorIntervalMs - tDiff), Timeout.Infinite);
                    }
                }
            }
            else
            {
                _gamepadEvents.Enqueue(GamepadInternalEvent.CreateDisconnectedEvent(Id));
            }
        }

        private void SendEventThread()
        {
            while (!_disposedValue)
            {
                if (_gamepadEvents.TryDequeue(out var gamepadEvent))
                {
                    switch (gamepadEvent.EventType)
                    {
                        case GamepadEventType.Disconnected:
                            GamepadDisconnected?.Invoke(this, EventArgs.Empty);
                            break;

                        case GamepadEventType.BatteryLevel:
                            NewGamepadBatteryLevel?.Invoke(this, gamepadEvent.GamepadBatteryLevel);
                            break;

                        case GamepadEventType.Input:
                            foreach (var (button, state) in gamepadEvent.DigitalInput)
                                NewGamepadInput?.Invoke(this, new ButtonEvent(button, state));

                            foreach (var (trigger, isStick, valueXOrSingle, valueY) in gamepadEvent.AnalogInput)
                            {
                                NewGamepadInput?.Invoke(this, isStick ?
                                    new StickEvent(trigger, valueXOrSingle, valueY) :
                                    new TriggerEvent(trigger, valueXOrSingle));
                            }
                            break;
                    }
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        #region Disposal
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    lock (_disposalLock)
                    {
                        _checkBatteryLevelTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        _checkControllerStateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        _disposedValue = true;
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #endregion
    }
}
