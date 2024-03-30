using System;
using PokerTracker3000.GameSession;
using PokerTracker3000.Interfaces;

namespace PokerTracker3000.Common.Messages
{
    public class StageChangedMessage : IInternalMessage
    {
        public int StageNumber { get; init; }

        public decimal SmallBlind { get; init; }

        public decimal BigBlind { get; init; }

        public int StageNumberOfSeconds { get; init; }

        public bool IsPause { get; init; }

        public bool CloneMessage => false;

        public static StageChangedMessage Create(GameStage stage)
            => new()
            {
                StageNumber = stage.Number,
                SmallBlind = stage.SmallBlind,
                BigBlind = stage.BigBlind,
                StageNumberOfSeconds = stage.LengthSeconds,
                IsPause = stage.IsPause,
            };
        public object Clone()
            => throw new NotImplementedException();
    }
}
