namespace PokerTracker3000.Spotify.ApiCalls.Shared
{
    public readonly struct FollowersObject
    {
        /// <summary>
        /// This will always be set to null, as the Web API does not support it at the moment.
        /// </summary>
        public string? Href { get; init; }

        /// <summary>
        /// The total number of followers.
        /// </summary>
        public int Total { get; init; }
    }
}