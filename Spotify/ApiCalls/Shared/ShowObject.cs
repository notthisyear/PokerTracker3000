namespace PokerTracker3000.Spotify.ApiCalls.Shared
{
    public readonly struct ShowObject
    {
        /// <summary>
        /// A list of the countries in which the show can be played, identified by their <a href="http://en.wikipedia.org/wiki/ISO_3166-1_alpha-2">ISO 3166-1 alpha-2</a> code.
        /// </summary>
        public string[]? AvailableMarkets { get; init; }

        /// <summary>
        /// The copyright statements of the show.
        /// </summary>
        public CopyrightObject[]? Copyrights { get; init; }

        /// <summary>
        /// A description of the show.
        /// <para>HTML tags are stripped away from this field, use html_description field in case HTML tags are needed.</para>
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// A description of the show. This field may contain HTML tags.
        /// </summary>
        public string? HtmlDescription { get; init; }

        /// <summary>
        /// Whether or not the show has explicit content (true = yes it does; false = no it does not OR unknown).
        /// </summary>
        public bool Explicit { get; init; }

        /// <summary>
        /// External URLs for this show.
        /// </summary>
        public ExternalUrlsObject ExternalUrls { get; init; }

        // <summary>
        /// A link to the Web API endpoint providing full details of the show.
        /// </summary>
        public string? Href { get; init; }

        /// <summary>
        /// The <a href="https://developer.spotify.com/documentation/web-api/concepts/spotify-uris-ids">Spotify ID</a> for the show.
        /// </summary>
        public string? Id { get; init; }

        /// <summary>
        /// The cover art for the show in various sizes, widest first.
        /// </summary>
        public ImageObject[] Images { get; init; }

        /// <summary>
        /// True if the episode is hosted outside of Spotify's CDN. This field might be null in some cases.
        /// </summary>
        public bool? IsExternallyHosted { get; init; }

        /// <summary>
        /// A list of the languages used in the show, identified by their <a href="https://en.wikipedia.org/wiki/ISO_639">ISO 639-1</a> code.
        /// </summary>
        public string[]? Languages { get; init; }

        /// <summary>
        /// The media type of the show.
        /// </summary>
        public string? MediaType { get; init; }

        /// <summary>
        /// The name of the episode.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// The publisher of the show.
        /// </summary>
        public string? Publisher { get; init; }

        /// <summary>
        /// The object type.
        /// </summary>
        public string? Type { get; init; }

        /// <summary>
        /// The <a href="https://developer.spotify.com/documentation/web-api/concepts/spotify-uris-ids">Spotify URI</a> for the show.
        /// </summary>
        public string? Uri { get; init; }

        /// <summary>
        /// The total number of episodes in the show.
        /// </summary>
        public int TotalEpisodes { get; init; }
    }
}