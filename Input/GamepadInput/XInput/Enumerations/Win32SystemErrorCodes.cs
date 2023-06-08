namespace PokerTracker3000.GamepadInput.XInput.Enumerations
{
    // Note: Not a complete list, only the ones relevant for XInput interaction added.
    internal enum Win32SystemErrorCodes : uint
    {
        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        ERROR_SUCCESS = 0x00,

        /// <summary>
        /// The device is not connected.
        /// </summary>
        ERROR_DEVICE_NOT_CONNECTED = 0x48f,

        Undefined = 0xffffffff
    }
}
