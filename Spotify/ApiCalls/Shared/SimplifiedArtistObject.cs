namespace PokerTracker3000.Spotify.ApiCalls.Shared
{
    public readonly struct SimplifiedArtistObject
    {
        /// <summary>
        /// External URLs for this context.
        /// </summary>
        public ExternalUrlsObject ExternalUrls { get; init; }

        /// <summary>
        /// A link to the Web API endpoint providing full details of the artist.
        /// </summary>
        public string? Href { get; init; }

        /// <summary>
        /// The <a href="https://developer.spotify.com/documentation/web-api/concepts/spotify-uris-ids">Spotify ID</a> for the artist.
        /// </summary>
        public string? Id { get; init; }

        /// <summary>
        /// The name of the artist.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// The object type.
        /// </summary>
        public string? Type { get; init; }

        /// <summary>
        /// The <a href="https://developer.spotify.com/documentation/web-api/concepts/spotify-uris-ids">Spotify URI</a> for the artist.
        /// </summary>
        public string? Uri { get; init; }
    }
}