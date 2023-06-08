using System.Runtime.InteropServices;
using PokerTracker3000.GamepadInput.XInput.Enumerations;

namespace PokerTracker3000.GamepadInput.XInput
{
    /// <summary>
    /// Describes the capabilities of a connected controller.
    /// </summary>
    /// <remarks>
    /// <para><see cref="XInput.XInputGetState"></see> returns XINPUT_CAPABILITIES to indicate the characteristics and available functionality of a specified controller.</para>
    /// <para>XInputGetCapabilities sets the structure members to indicate which inputs the device supports.
    /// For binary state controls, such as digital buttons, the corresponding bit reflects whether or not the control is supported by the device.
    /// For proportional controls, such as thumbsticks, the value indicates the resolution for that control.
    /// Some number of the least significant bits may not be set, indicating that the control does not provide resolution to that level.</para>
    /// <para>The SubType member indicates the specific subtype of controller present.
    /// Games may detect the controller subtype and tune their handling of controller input or output based on subtypes
    /// that are well suited to their game genre. For example, a car racing game might check for the presence of a wheel controller
    /// to provide finer control of the car being driven.However, titles must not disable or ignore a device based on its subtype.
    /// Subtypes not recognized by the game or for which the game is not specifically tuned should be treated as a standard
    /// Xbox 360 Controller (XINPUT_DEVSUBTYPE_GAMEPAD).</para>
    /// <para>Older XUSB Windows drivers report incomplete capabilities information, particularly for wireless devices.
    /// The latest XUSB Windows driver provides full support for wired and wireless devices, and more complete and accurate capabilities flags.</para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XINPUT_CAPABILITIES
    {
        /// <summary>
        /// Controller type. See <seealso cref="XInputControllerType"/> for allowed values.
        /// </summary>
        public byte Type;

        /// <summary>
        /// Subtype of the game controller. See <seealso cref="XInputControllerSubtype"/> for allowed values.
        /// </summary>
        public byte SubType;

        /// <summary>
        /// Features of the controller. <see cref="XInputControllerFeatureFlag"/> for allowed values.
        /// </summary>
        public ushort Flags;

        /// <summary>
        /// <see cref="XINPUT_GAMEPAD"/> structure that describes available controller features and control resolutions.
        /// </summary>
        public XINPUT_GAMEPAD Gamepad;

        /// <summary>
        /// <see cref="XINPUT_VIBRATION"/> structure that describes available vibration functionality and resolutions.
        /// </summary>
        public XINPUT_VIBRATION Vibration;
    }
}
