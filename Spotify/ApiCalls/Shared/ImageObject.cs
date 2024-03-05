namespace PokerTracker3000.Spotify.ApiCalls.Shared
{
    public readonly struct ImageObject
    {
        /// <summary>
        /// The source URL of the image.
        /// </summary>
        public string? Url { get; init; }

        /// <summary>
        /// The source URL of the image.
        /// </summary>
        public int? Height { get; init; }

        /// <summary>
        /// The image width in pixels.
        /// </summary>
        public int? Width { get; init; }
    }
}