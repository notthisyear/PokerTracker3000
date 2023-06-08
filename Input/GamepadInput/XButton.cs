using System;
using static PokerTracker3000.GamepadInput.XInput.Enumerations.XInputGamepadDigitalButtonFlag;

namespace PokerTracker3000.GamepadInput
{
    /// <summary>
    /// Exposes constants that represent a button of an 
    /// XInput controller. These constants can be used 
    /// as bitwise flags to represent several buttons.
    /// </summary>
    [Flags]
    public enum XButton
    {
        /// <summary>
        /// No button.
        /// </summary>
        None = 0,

        /// <summary>
        /// D-Pad Up.
        /// </summary>
        DPadUp = XINPUT_GAMEPAD_DPAD_UP,

        /// <summary>
        /// D-Pad Down.
        /// </summary>
        DPadDown = XINPUT_GAMEPAD_DPAD_DOWN,

        /// <summary>
        /// D-Pad Left.
        /// </summary>
        DPadLeft = XINPUT_GAMEPAD_DPAD_LEFT,

        /// <summary>
        /// D-Pad Right.
        /// </summary>
        DPadRight = XINPUT_GAMEPAD_DPAD_RIGHT,

        /// <summary>
        /// The Start button.
        /// </summary>
        Start = XINPUT_GAMEPAD_START,

        /// <summary>
        /// The Back button.
        /// </summary>
        Back = XINPUT_GAMEPAD_BACK,

        /// <summary>
        /// The LS (Left Stick) button.
        /// </summary>
        LS = XINPUT_GAMEPAD_LEFT_THUMB,

        /// <summary>
        /// The RS (Right Stick) button.
        /// </summary>
        RS = XINPUT_GAMEPAD_RIGHT_THUMB,

        /// <summary>
        /// The LB (Left Shoulder) button.
        /// </summary>
        LB = XINPUT_GAMEPAD_LEFT_SHOULDER,

        /// <summary>
        /// The RB (Right Shoulder).
        /// </summary>
        RB = XINPUT_GAMEPAD_RIGHT_SHOULDER,

        /// <summary>
        /// The A button.
        /// </summary>
        A = XINPUT_GAMEPAD_A,

        /// <summary>
        /// The B button.
        /// </summary>
        B = XINPUT_GAMEPAD_B,

        /// <summary>
        /// The X button.
        /// </summary>
        X = XINPUT_GAMEPAD_X,

        /// <summary>
        /// The Y button.
        /// </summary>
        Y = XINPUT_GAMEPAD_Y,

    }
}
