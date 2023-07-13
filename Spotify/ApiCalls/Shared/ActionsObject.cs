namespace PokerTracker3000.Spotify.ApiCalls.Shared
{
    public readonly struct DisallowsObject
    {
        /// <summary>
        /// Resuming. Optional field.
        /// </summary>
        public bool? Resuming { get; init; }

        /// <summary>
        /// Skipping to the previous context. Optional field.
        /// </summary>
        public bool? SkippingPrev { get; init; }
    }
    public readonly struct ActionsObject
    {
        /// <summary>
        /// This seems to be what is actually returned...? Cannot find any official documentation pointing to this though?
        ///</summary>
        public DisallowsObject Disallows { get; init; }

        /// <summary>
        /// Interrupting playback. Optional field.
        /// </summary>
        public bool? InterruptingPlayback { get; init; }

        /// <summary>
        /// Pausing. Optional field.
        /// </summary>
        public bool? Pausing { get; init; }

        /// <summary>
        /// Resuming. Optional field.
        /// </summary>
        public bool? Resuming { get; init; }

        /// <summary>
        /// Seeking playback location. Optional field.
        /// </summary>
        public bool? Seeking { get; init; }

        /// <summary>
        /// Skipping to the next context. Optional field.
        /// </summary>
        public bool? SkippingNext { get; init; }

        /// <summary>
        /// Skipping to the previous context. Optional field.
        /// </summary>
        public bool? SkippingPrev { get; init; }

        /// <summary>
        /// Toggling repeat context flag. Optional field.
        /// </summary>
        public bool? TogglingRepeatContext { get; init; }

        /// <summary>
        /// Toggling shuffle flag. Optional field.
        /// </summary>
        public bool? TogglingShuffle { get; init; }

        /// <summary>
        /// Toggling repeat track flag. Optional field.
        /// </summary>
        public bool? TogglingRepeatTrack { get; init; }

        /// <summary>
        /// Transfering playback between devices. Optional field.
        /// </summary>
        public bool? TransferringPlayback { get; init; }
    }
}