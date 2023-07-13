namespace PokerTracker3000.Spotify.ApiCalls.Shared
{
    public readonly struct ExplicitContent
    {
        /// <summary>
        /// When true, indicates that explicit content should not be played.
        /// </summary>
        public bool FilterEnabled { get; init; }

        /// <summary>
        /// When true, indicates that the explicit content setting is locked and can't be changed by the user.
        /// </summary>
        public bool FilterLocked { get; init; }
    }
}