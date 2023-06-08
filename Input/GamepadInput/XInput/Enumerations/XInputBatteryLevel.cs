namespace PokerTracker3000.GamepadInput.XInput.Enumerations
{
    internal enum BatteryLevel : byte
    {
        // Charge is between zero and 10%.
        BATTERY_LEVEL_EMPTY = 0x00,

        // Charge is between 10% and 40%.
        BATTERY_LEVEL_LOW = 0x01,

        // Charge is between 40% and 70%.
        BATTERY_LEVEL_MEDIUM = 0x02,

        // Charge is between 70% and 100%.
        BATTERY_LEVEL_FULL = 0x03,

        Undefined = 0xff
    }
}
