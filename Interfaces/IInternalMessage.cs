using System;

namespace PokerTracker3000.Interfaces
{
    public interface IInternalMessage : ICloneable
    {
        public bool CloneMessage { get; }
    }
}
