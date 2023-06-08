using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using PokerTracker3000.GamepadInput.XInput;
using PokerTracker3000.GamepadInput.XInput.Enumerations;

namespace PokerTracker3000.GamepadInput
{
    public enum GamepadBatteryLevel
    {
        Empty,
        Low,
        Medium,
        Full
    }

    internal class Gamepad
    {
        #region Events
        public event EventHandler? GamepadDisconnected;
        public event EventHandler? NewGamepadInput;
        public event EventHandler<GamepadBatteryLevel>? NewGamepadBatteryLevel;
        #endregion

        #region Public properties
        public uint Id { get; }

        public bool IsWireless { get; private set; }

        public bool CanGetBatteryLevel { get; private set; }
        #endregion

        #region Private fields
        private uint _lastPacketNumber = uint.MaxValue;
        private BatteryLevel _lastBatteryLevel = BatteryLevel.BATTERY_LEVEL_EMPTY;
        private BatteryType _batteryType;
        private bool _sendBatteryLevelUpdate = false;
        private int _msSinceLastBatteryCheck = 0;
        private readonly Thread _gamepadStateMonitor;

        private const int GamepadStateMonitorIntervalMs = 2000;
        private const int CheckBatteryStateIntervalMs = 300000;
        bool b = true;
        #endregion

        public Gamepad(uint id)
        {
            Id = id;
            _gamepadStateMonitor = new Thread(CheckGamepadState) { IsBackground = true };
            if (GetAndSetControllerCapabilities())
                _gamepadStateMonitor.Start();
        }

        #region Private methods
        private bool GetAndSetControllerCapabilities()
        {
            XINPUT_CAPABILITIES capabilities = new();
            if (ImportedMethods.XInputGetCapabilities(Id, XInputControllerTypeFlag.XINPUT_FLAG_GAMEPAD.ToUnsignedInteger(), ref capabilities).ToErrorCode() != Win32SystemErrorCodes.ERROR_SUCCESS)
                return false;


            if (capabilities.Type.ToControllerType() != XInputControllerType.XINPUT_DEVTYPE_GAMEPAD ||
                capabilities.SubType.ToControllerSubtype() != XInputControllerSubtype.XINPUT_DEVSUBTYPE_GAMEPAD)
            {
                return false;
            }

            IsWireless = capabilities.Flags.GetControllerFeatureFlags().Where(x => x == XInputControllerFeatureFlag.XINPUT_CAPS_WIRELESS).Any();


            XINPUT_BATTERY_INFORMATION batteryInformation = new();
            if (ImportedMethods.XInputGetBatteryInformation(Id, XInputBatteryDeviceType.BATTERY_DEVTYPE_GAMEPAD.ToUnsignedByte(), ref batteryInformation).ToErrorCode() == Win32SystemErrorCodes.ERROR_SUCCESS)
            {
                _batteryType = batteryInformation.BatteryType.ToBatteryType();
                CanGetBatteryLevel = _batteryType == BatteryType.BATTERY_TYPE_ALKALINE || _batteryType == BatteryType.BATTERY_TYPE_NIMH;
                if (CanGetBatteryLevel)
                {
                    _lastBatteryLevel = batteryInformation.BatteryLevel.ToBatteryLevel();
                    _sendBatteryLevelUpdate = true;
                }
            }

            return true;
        }

        private void CheckGamepadState()
        {
            if (_msSinceLastBatteryCheck > CheckBatteryStateIntervalMs)
            {
                XINPUT_BATTERY_INFORMATION batteryInformation = new();
                if (ImportedMethods.XInputGetBatteryInformation(Id, XInputBatteryDeviceType.BATTERY_DEVTYPE_GAMEPAD.ToUnsignedByte(), ref batteryInformation).ToErrorCode() == Win32SystemErrorCodes.ERROR_SUCCESS)
                {
                    if (batteryInformation.BatteryLevel.ToBatteryLevel() != _lastBatteryLevel)
                    {
                        _lastBatteryLevel = batteryInformation.BatteryLevel.ToBatteryLevel();
                        _sendBatteryLevelUpdate = true;
                    }
                }
                _msSinceLastBatteryCheck = 0;
            }

            if (_sendBatteryLevelUpdate)
            {
                NewGamepadBatteryLevel?.Invoke(this, MapToGamepadBatteryLevel(_lastBatteryLevel));
                _sendBatteryLevelUpdate = false;
            }

            XINPUT_STATE state = new();
            if (ImportedMethods.XInputGetState(Id, ref state).ToErrorCode() == XInput.Enumerations.Win32SystemErrorCodes.ERROR_SUCCESS)
            {
                if (state.dwPacketNumber != _lastPacketNumber)
                {
                    _lastPacketNumber = state.dwPacketNumber;
                }

                Thread.Sleep(GamepadStateMonitorIntervalMs);
                _msSinceLastBatteryCheck += GamepadStateMonitorIntervalMs;
                CheckGamepadState();
            }
            else
            {
                GamepadDisconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        private static GamepadBatteryLevel MapToGamepadBatteryLevel(BatteryLevel batteryLevel)
            => batteryLevel switch
            {
                BatteryLevel.BATTERY_LEVEL_EMPTY => GamepadBatteryLevel.Empty,
                BatteryLevel.BATTERY_LEVEL_LOW => GamepadBatteryLevel.Low,
                BatteryLevel.BATTERY_LEVEL_MEDIUM => GamepadBatteryLevel.Medium,
                BatteryLevel.BATTERY_LEVEL_FULL => GamepadBatteryLevel.Full,
                _ => throw new NotImplementedException(),
            };

        #endregion
        private static void PrintGamepad(XINPUT_GAMEPAD pad)
        {
            Debug.WriteLine($"sThumbLX: {pad.sThumbLX}");
            Debug.WriteLine($"sThumbLY: {pad.sThumbLY}");
            Debug.WriteLine($"sThumbRX: {pad.sThumbRX}");
            Debug.WriteLine($"sThumbRY: {pad.sThumbRY}");
            Debug.WriteLine($"wButtons: {pad.wButtons}");
            Debug.WriteLine($"bLeftTrigger: {pad.bLeftTrigger}");
            Debug.WriteLine($"bRightTrigger: {pad.bRightTrigger}");
        }
    }
}
