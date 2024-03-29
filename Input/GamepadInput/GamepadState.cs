using PokerTracker3000.GamepadInput.XInput;
using PokerTracker3000.GamepadInput.XInput.Enumerations;

namespace PokerTracker3000.GamepadInput
{
    internal record GamepadState
    {
        public uint Id { get; init; }

        public bool DPadUpPressed { get; init; }

        public bool DPadDownPressed { get; init; }

        public bool DPadLeftPressed { get; init; }

        public bool DPadRightPressed { get; init; }

        public bool StartButtonPressed { get; init; }

        public bool BackButtonPressed { get; init; }

        public bool LeftShoulderPressed { get; init; }

        public bool RightShoulderPressed { get; init; }

        public bool LeftThumbPressed { get; init; }

        public bool RightThumbPressed { get; init; }

        public bool AButtonPressed { get; init; }

        public bool BButtonPressed { get; init; }

        public bool XButtonPressed { get; init; }

        public bool YButtonPressed { get; init; }

        public byte LeftAnalogTrigger { get; init; }

        public byte RightAnalogTrigger { get; init; }

        public short LeftStickX { get; init; }

        public short LeftStickY { get; init; }

        public short RightStickX { get; init; }

        public short RightStickY { get; init; }

        public static GamepadState Create(uint id, XINPUT_GAMEPAD gamepad)
            => new()
            {
                Id = id,
                DPadUpPressed = HasFlag(gamepad.wButtons, (ushort)XInputGamepadDigitalButtonFlag.XINPUT_GAMEPAD_DPAD_UP),
                DPadDownPressed = HasFlag(gamepad.wButtons, (ushort)XInputGamepadDigitalButtonFlag.XINPUT_GAMEPAD_DPAD_DOWN),
                DPadLeftPressed = HasFlag(gamepad.wButtons, (ushort)XInputGamepadDigitalButtonFlag.XINPUT_GAMEPAD_DPAD_LEFT),
                DPadRightPressed = HasFlag(gamepad.wButtons, (ushort)XInputGamepadDigitalButtonFlag.XINPUT_GAMEPAD_DPAD_RIGHT),
                StartButtonPressed = HasFlag(gamepad.wButtons, (ushort)XInputGamepadDigitalButtonFlag.XINPUT_GAMEPAD_START),
                BackButtonPressed = HasFlag(gamepad.wButtons, (ushort)XInputGamepadDigitalButtonFlag.XINPUT_GAMEPAD_BACK),
                LeftShoulderPressed = HasFlag(gamepad.wButtons, (ushort)XInputGamepadDigitalButtonFlag.XINPUT_GAMEPAD_LEFT_SHOULDER),
                RightShoulderPressed = HasFlag(gamepad.wButtons, (ushort)XInputGamepadDigitalButtonFlag.XINPUT_GAMEPAD_RIGHT_SHOULDER),
                LeftThumbPressed = HasFlag(gamepad.wButtons, (ushort)XInputGamepadDigitalButtonFlag.XINPUT_GAMEPAD_LEFT_THUMB),
                RightThumbPressed = HasFlag(gamepad.wButtons, (ushort)XInputGamepadDigitalButtonFlag.XINPUT_GAMEPAD_RIGHT_THUMB),
                AButtonPressed = HasFlag(gamepad.wButtons, (ushort)XInputGamepadDigitalButtonFlag.XINPUT_GAMEPAD_A),
                BButtonPressed = HasFlag(gamepad.wButtons, (ushort)XInputGamepadDigitalButtonFlag.XINPUT_GAMEPAD_B),
                XButtonPressed = HasFlag(gamepad.wButtons, (ushort)XInputGamepadDigitalButtonFlag.XINPUT_GAMEPAD_X),
                YButtonPressed = HasFlag(gamepad.wButtons, (ushort)XInputGamepadDigitalButtonFlag.XINPUT_GAMEPAD_Y),
                LeftAnalogTrigger = gamepad.bLeftTrigger,
                RightAnalogTrigger = gamepad.bRightTrigger,
                LeftStickX = gamepad.sThumbLX,
                LeftStickY = gamepad.sThumbLY,
                RightStickX = gamepad.sThumbRX,
                RightStickY = gamepad.sThumbRY,
            };

        private static bool HasFlag(ushort v, ushort flag)
            => (v & flag) > 0x00;
    }
}
