namespace PokerTracker3000.Spotify.ApiCalls.Shared
{
    public readonly struct CopyrightObject
    {
        /// <summary>
        /// The copyright text for this content.
        /// </summary>
        public string? Text { get; init; }

        /// <summary>
        /// The type of copyright: C = the copyright, P = the sound recording (performance) copyright.
        /// </summary>
        public string? Type { get; init; }
    }
}