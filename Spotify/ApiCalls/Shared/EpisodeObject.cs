namespace PokerTracker3000.Spotify.ApiCalls.Shared
{
    // Note: This is a class instead of a readonly struct so that it can be used in a OneOf<T1, T2> property.
    public class EpisodeObject
    {
        /// <summary>
        /// A URL to a 30 second preview (MP3 format) of the episode. null if not available.
        /// </summary>
        public string? AudioPreviewUrl { get; init; }

        /// <summary>
        /// A description of the episode. HTML tags are stripped away from this field, use html_description field in case HTML tags are needed.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// A description of the episode. This field may contain HTML tags.
        /// </summary>
        public string? HtmlDescription { get; init; }

        /// <summary>
        /// The episode length in milliseconds.
        /// </summary>
        public int DurationMs { get; init; }

        /// <summary>
        /// Whether or not the episode has explicit content (true = yes it does; false = no it does not OR unknown).
        /// </summary>
        public bool Explicit { get; init; }

        /// <summary>
        /// External URLs for this episode.
        /// </summary>

        public ExternalUrlsObject ExternalUrls { get; init; }

        /// <summary>
        /// A link to the Web API endpoint providing full details of the episode.
        /// </summary>
        public string? Href { get; init; }

        /// <summary>
        /// The <a href="https://developer.spotify.com/documentation/web-api/concepts/spotify-uris-ids">Spotify ID</a> for the episode.
        /// </summary>
        public string? Id { get; init; }

        /// <summary>
        /// The cover art for the episode in various sizes, widest first.
        /// </summary>
        public ImageObject[]? Images { get; init; }

        /// <summary>
        /// True if the episode is hosted outside of Spotify's CDN.
        /// </summary>
        public bool IsExternallyHosted { get; init; }

        /// <summary>
        /// True if the episode is playable in the given market. Otherwise false.
        /// </summary>
        public bool IsPlayable { get; init; }

        /// <summary>
        /// The language used in the episode, identified by a ISO 639 code. This field is deprecated and might be removed in the future. Please use the languages field instead.
        /// </summary>
        public string? Language { get; init; }

        /// <summary>
        /// A list of the languages used in the episode, identified by their <a href="https://en.wikipedia.org/wiki/ISO_639">ISO 639-1</a> code.
        /// </summary>
        public string[]? Languages { get; init; }

        /// <summary>
        /// The name of the episode.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// The date the episode was first released, for example "1981-12-15". Depending on the precision, it might be shown as "1981" or "1981-12".
        /// </summary>
        public string? ReleaseDate { get; init; }

        /// <summary>
        /// The precision with which release_date value is known.
        /// </summary>
        public string? ReleaseDatePrecision { get; init; }

        /// <summary>
        /// The user's most recent position in the episode. Set if the supplied access token is a user token and has the scope 'user-read-playback-position'.
        /// </summary>
        public ResumePointObject ResumePoint { get; init; }

        /// <summary>
        /// The object type.
        /// </summary>
        public string? Type { get; init; }

        /// <summary>
        /// The <a href="https://developer.spotify.com/documentation/web-api/concepts/spotify-uris-ids">Spotify URI</a> for the episode.
        /// </summary>
        public string? Uri { get; init; }

        /// <summary>
        /// Included in the response when a content restriction is applied.
        /// </summary>
        public RestrictionsObject? Restrictions { get; init; }

        /// <summary>
        /// The show on which the episode belongs.
        /// </summary>
        public ShowObject Show { get; init; }

    }
}