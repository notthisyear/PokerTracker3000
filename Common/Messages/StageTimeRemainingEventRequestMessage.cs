using PokerTracker3000.Interfaces;

namespace PokerTracker3000.Common.Messages
{
    public enum RequestType
    {
        None,
        AddEvent,
        RemoveEvent
    }

    public class StageTimeRemainingEventRequestMessage : IInternalMessage
    {
        public RequestType Type { get; init; }

        public int EventAtTimeSeconds { get; init; } = 0;

        public bool CloneMessage => true;

        public object Clone()
            => new StageTimeRemainingEventRequestMessage() { Type = Type, EventAtTimeSeconds = EventAtTimeSeconds };
    }
}
