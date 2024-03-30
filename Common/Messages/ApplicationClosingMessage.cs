using System;
using PokerTracker3000.Interfaces;

namespace PokerTracker3000.Common.Messages
{
    public class ApplicationClosingMessage : IInternalMessage
    {
        public bool CloneMessage => false;

        public object Clone()
            => throw new NotImplementedException();
    }
}
