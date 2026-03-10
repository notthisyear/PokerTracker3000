using PokerTracker3000.Interfaces;

namespace PokerTracker3000.Common.Messages
{

    public class StageTimeRemainingEventMessage : IInternalMessage
    {
        public int EventTimeSeconds { get; init; }

        public bool CloneMessage => true;

        public object Clone()
            => new StageTimeRemainingEventMessage() { EventTimeSeconds = EventTimeSeconds };
    }
}
