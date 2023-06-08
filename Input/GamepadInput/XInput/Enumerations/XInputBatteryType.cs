namespace PokerTracker3000.GamepadInput.XInput.Enumerations
{
    public enum BatteryType : byte
    {
        //The device is not connected.
        BATTERY_TYPE_DISCONNECTED = 0x00,

        //The device is a wired device and does not have a battery.
        BATTERY_TYPE_WIRED = 0x01,

        //The device has an alkaline battery.
        BATTERY_TYPE_ALKALINE = 0x02,

        //The device has a nickel metal hydride battery.
        BATTERY_TYPE_NIMH = 0x03,

        //The device has an unknown battery type
        BATTERY_TYPE_UNKNOWN = 0xff
    }
}
