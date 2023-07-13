namespace PokerTracker3000.Spotify.ApiCalls.Shared
{
    public readonly struct ArtistObject
    {
        /// <summary>
        /// External URLs for this context.
        /// </summary>
        public ExternalUrlsObject ExternalUrls { get; init; }

        /// <summary>
        /// Information about the followers of the artist.
        /// </summary>
        public FollowersObject Followers { get; init; }

        /// <summary>
        /// A list of the genres the artist is associated with. If not yet classified, the array is empty.
        /// </summary>
        public string[]? Genres { get; init; }

        /// <summary>
        /// A link to the Web API endpoint providing full details of the artist.
        /// </summary>
        public string? Href { get; init; }

        /// <summary>
        /// The <a href="https://developer.spotify.com/documentation/web-api/concepts/spotify-uris-ids">Spotify ID</a> for the artist.
        /// </summary>
        public string? Id { get; init; }

        /// <summary>
        /// Images of the artist in various sizes, widest first.
        /// </summary>
        public ImageObject[] Images { get; init; }

        /// <summary>
        /// The name of the artist.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// The popularity of the artist.
        /// <para>The value will be between 0 and 100, with 100 being the most popular.
        /// The artist's popularity is calculated from the popularity of all the artist's tracks.</para>
        /// </summary>
        public int? Popularity { get; init; }

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