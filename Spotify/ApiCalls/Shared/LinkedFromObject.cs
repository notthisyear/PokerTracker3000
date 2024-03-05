namespace PokerTracker3000.Spotify.ApiCalls.Shared
{
    public readonly struct LinkedFromObject
    {
        /// <summary>
        /// Known external URLs for this track.
        /// </summary>
        public ExternalUrlsObject ExternalUrls { get; init; }

        /// <summary>
        /// KA link to the Web API endpoint providing full details of the track.
        /// </summary>
        public string? Href { get; init; }

        /// <summary>
        /// The <a href="https://developer.spotify.com/documentation/web-api/concepts/spotify-uris-ids">Spotify ID</a> for the track.
        /// </summary>
        public string? Id { get; init; }

        /// <summary>
        /// The object type.
        /// </summary>
        public string? Type { get; init; }

        /// <summary>
        /// The <a href="https://developer.spotify.com/documentation/web-api/concepts/spotify-uris-ids">Spotify URI</a> for the album.
        /// </summary>
        public string? Uri { get; init; }
    }
}