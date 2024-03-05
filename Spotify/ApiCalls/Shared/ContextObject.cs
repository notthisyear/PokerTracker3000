namespace PokerTracker3000.Spotify.ApiCalls.Shared
{
    public readonly struct ContextObject
    {
        /// <summary>
        /// The object type, e.g. "artist", "playlist", "album", "show".
        /// </summary>
        public string? Type { get; init; }

        /// <summary>
        /// A link to the Web API endpoint providing full details of the track.
        /// </summary>
        public string? Href { get; init; }

        /// <summary>
        /// External URLs for this context.
        /// </summary>
        public ExternalUrlsObject ExternalUrls { get; init; }

        /// <summary>
        /// The <a href="https://developer.spotify.com/documentation/web-api/concepts/spotify-uris-ids">Spotify URI</a> for the context.
        /// </summary>
        public string? Uri { get; init; }
    }
}