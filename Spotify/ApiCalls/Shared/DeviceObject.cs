namespace PokerTracker3000.Spotify.ApiCalls.Shared
{
    public readonly struct DeviceObject
    {
        /// <summary>
        /// The device ID.
        /// </summary>
        public string? Id { get; init; }

        /// <summary>
        /// If this device is the currently active device.
        /// </summary>
        public bool IsActive { get; init; }

        /// <summary>
        /// If this device is currently in a private session.
        /// </summary>
        public bool IsPrivateSession { get; init; }

        /// <summary>
        /// Whether controlling this device is restricted.
        /// <para>At present if this is "true" then no Web API commands will be accepted by this device.</para>
        /// </summary>
        public bool IsRestricted { get; init; }

        /// <summary>
        /// A human-readable name for the device.
        /// <para>Some devices have a name that the user can configure (e.g. "Loudest speaker") and some devices have a generic name associated with the manufacturer or device model.</para>
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Device type, such as "computer", "smartphone" or "speaker".
        /// </summary>
        public string? Type { get; init; }

        /// <summary>
        /// The current volume in percent.
        /// </summary>
        public int? VolumePercent { get; init; }
    }
}