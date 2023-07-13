namespace PokerTracker3000.Spotify.ApiCalls.Shared
{
    public readonly struct AlbumObject
    {
        /// <summary>
        /// The type of the album.
        /// </summary>
        public string? AlbumType { get; init; }

        /// <summary>
        /// The number of tracks in the album.
        /// </summary>
        public int TotalTracks { get; init; }

        /// <summary>
        /// The markets in which the album is available: <a href="http://en.wikipedia.org/wiki/ISO_3166-1_alpha-2">ISO 3166-1 alpha-2</a> country codes.
        /// <para>NOTE: an album is considered available in a market when at least 1 of its tracks is available in that market.</para>
        /// </summary>
        public string[]? AvailableMarkets { get; init; }

        /// <summary>
        /// Known external URLs for this album.
        /// </summary>
        public ExternalUrlsObject ExternalUrls { get; init; }

        /// <summary>
        /// A link to the Web API endpoint providing full details of the album.
        /// </summary>
        public string? Href { get; init; }

        /// <summary>
        /// The <a href="https://developer.spotify.com/documentation/web-api/concepts/spotify-uris-ids">Spotify ID</a> for the album.
        /// </summary>
        public string? Id { get; init; }

        /// <summary>
        /// The cover art for the album in various sizes, widest first.
        /// </summary>
        public ImageObject[] Images { get; init; }

        /// <summary>
        /// The name of the album. In case of an album takedown, the value may be an empty string.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// The date the album was first released.
        /// </summary>
        public string? ReleaseDate { get; init; }

        /// <summary>
        /// The precision with which release_date value is known.
        /// </summary>
        public string? ReleaseDatePrecision { get; init; }

        /// <summary>
        ///Included in the response when a content restriction is applied.
        /// </summary>
        public RestrictionsObject? Restricions { get; init; }

        /// <summary>
        /// The object type.
        /// </summary>
        public string? Type { get; init; }

        /// <summary>
        /// The <a href="https://developer.spotify.com/documentation/web-api/concepts/spotify-uris-ids">Spotify URI</a> for the album.
        /// </summary>
        public string? Uri { get; init; }

        /// <summary>
        /// The copyright statements of the album.
        /// </summary>
        public CopyrightObject[]? Copyrights { get; init; }

        /// <summary>
        /// Known external IDs for the album.
        /// </summary>
        public ExternalIdsObject ExternalIds { get; init; }

        /// <summary>
        /// A list of the genres the album is associated with. If not yet classified, the array is empty.
        /// </summary>
        public string[] Genres { get; init; }

        /// <summary>
        /// The label associated with the album.
        /// </summary>
        public string? Label { get; init; }

        /// <summary>
        /// The popularity of the album. The value will be between 0 and 100, with 100 being the most popular.
        /// </summary>
        public int? Popularity { get; init; }

        /// <summary>
        /// The field is present when getting an artist's albums. Compare to album_type this field represents relationship between the artist and the album.
        /// </summary>
        public string? AlbumGroup { get; init; }

        /// <summary>
        /// The artists of the album. Each artist object includes a link in href to more detailed information about the artist.
        /// </summary>
        public SimplifiedArtistObject[]? Artists { get; init; }
    }
}