using System.Runtime.InteropServices;

namespace PokerTracker3000.GamepadInput.XInput
{
    /// <summary>
    /// Describes the current state of the Xbox 360 Controller.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XINPUT_BATTERY_INFORMATION
    {
        /// <summary>
        /// The type of battery. See <seealso cref="Enumerations.BatteryType"/> for allowed values.
        /// </summary>
        public byte BatteryType;

        /// <summary>
        /// The charge state of the battery.
        /// This value is only valid for wireless devices with a known battery type. See <seealso cref="Enumerations.BatteryLevel"/> for allowed values.
        /// </summary>
        public byte BatteryLevel;
    }
}
