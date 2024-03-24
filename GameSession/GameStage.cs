using System;

namespace PokerTracker3000.GameSession
{
    public record GameStage(int Number, bool IsPause, decimal SmallBlind, decimal BigBlind, TimeSpan Length)
    {
        public string Name => $"Stage {Number}";
    }
}
