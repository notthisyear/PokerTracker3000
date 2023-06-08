using System;

namespace PokerTracker3000.GamepadInput.XInput.Enumerations
{
    [Flags]
    internal enum XInputControllerFeatureFlag : ushort
    {
        /// <summary>
        /// Device supports force feedback functionality.
        /// Note that these force-feedback features beyond rumble are not currently supported through XINPUT on Windows.
        /// </summary>
        XINPUT_CAPS_FFB_SUPPORTED = 0x0001,

        /// <summary>
        /// Device is wireless.
        /// </summary>
        XINPUT_CAPS_WIRELESS = 0x0002,

        /// <summary>
        /// Device has an integrated voice device.
        /// </summary>
        XINPUT_CAPS_VOICE_SUPPORTED = 0x04,

        /// <summary>
        /// Device supports plug-in modules.
        /// Note that plug-in modules like- the text input device (TID) are not supported currently through XINPUT on Windows.
        /// </summary>
        XINPUT_CAPS_PMD_SUPPORTED = 0x008,

        /// <summary>
        /// Device lacks menu navigation buttons (START, BACK, DPAD).
        /// </summary>
        XINPUT_CAPS_NO_NAVIGATION = 0x0010
    }
}
