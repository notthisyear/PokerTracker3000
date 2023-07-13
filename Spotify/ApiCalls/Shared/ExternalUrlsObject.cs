namespace PokerTracker3000.Spotify.ApiCalls.Shared
{
    public readonly struct ExternalUrlsObject
    {
        /// <summary>
        /// The <a href="https://developer.spotify.com/documentation/web-api/concepts/spotify-uris-ids">Spotify URL</a> for the object.
        /// </summary>
        public string? Spotify { get; init; }
    }
}