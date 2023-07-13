namespace PokerTracker3000.Spotify.ApiCalls.Shared
{
    // Note: This is a class instead of a readonly struct so that it can be used in a OneOf<T1, T2> property.
    public class TrackObject
    {
        /// <summary>
        /// The album on which the track appears.
        /// <para>The album object includes a link in href to full information about the album.</para>
        /// </summary>
        public AlbumObject Album { get; init; }

        /// <summary>
        /// The artists who performed the track.
        /// <para>Each artist object includes a link in href to more detailed information about the artist.</para>
        /// </summary>
        public ArtistObject[]? Artists { get; init; }

        /// <summary>
        /// A list of the countries in which the track can be played, identified by their <a href="http://en.wikipedia.org/wiki/ISO_3166-1_alpha-2">ISO 3166-1 alpha-2</a> code.
        /// </summary>
        public string[]? AvailableMarkets { get; init; }

        /// <summary>
        /// The disc number (usually 1 unless the album consists of more than one disc).
        /// </summary>
        public int DiscNumber { get; init; }

        /// <summary>
        /// The track length in milliseconds.
        /// </summary>
        public int DurationMs { get; init; }

        /// <summary>
        /// Whether or not the track has explicit lyrics ( true = yes it does; false = no it does not OR unknown).
        /// </summary>
        public bool Explicit { get; init; }

        /// <summary>
        /// Known external IDs for the track.
        /// </summary>
        public ExternalIdsObject ExternalIds { get; init; }

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
        /// Part of the response when Track Relinking is applied. If true, the track is playable in the given market. Otherwise false.
        /// </summary>
        public bool? IsPlayable { get; init; }

        /// <summary>
        /// Part of the response when Track Relinking is applied, and the requested track has been replaced with different track.
        /// The track in the linked_from object contains information about the originally requested track.
        /// </summary>
        public LinkedFromObject? LinkedFrom { get; init; }

        /// <summary>
        /// Included in the response when a content restriction is applied.
        /// </summary>
        public RestrictionsObject? Restriction { get; init; }

        /// <summary>
        /// The name of the track.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// The popularity of the album. The value will be between 0 and 100, with 100 being the most popular.
        /// </summary>
        public int Popularity { get; init; }

        /// <summary>
        /// A link to a 30 second preview (MP3 format) of the track. Can be null
        /// </summary>
        public string? PreviewUrl { get; init; }

        /// <summary>
        /// The number of the track. If an album has several discs, the track number is the number on the specified disc.
        /// </summary>
        public int TrackNumber { get; init; }

        /// <summary>
        /// The object type: "track".
        /// </summary>
        public string? Type { get; init; }

        /// <summary>
        /// The <a href="https://developer.spotify.com/documentation/web-api/concepts/spotify-uris-ids">Spotify URI</a> for the track.
        /// </summary>
        public string? Uri { get; init; }

        /// <summary>
        /// Whether or not the track is from a local file.
        /// </summary>
        public bool IsLocal { get; init; }
    }
}