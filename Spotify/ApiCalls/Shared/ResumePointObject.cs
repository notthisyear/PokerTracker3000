namespace PokerTracker3000.Spotify.ApiCalls.Shared
{
    public readonly struct ResumePointObject
    {
        /// <summary>
        /// Whether or not the episode has been fully played by the user.
        /// </summary>
        public bool FullyPlayed { get; init; }

        /// <summary>
        /// The user's most recent position in the episode in milliseconds.
        /// </summary>
        public int ResumePositionMs { get; init; }
    }
}