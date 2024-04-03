using System;
using PokerTracker3000.Interfaces;

namespace PokerTracker3000.Common.Messages
{
    public class ApplicationClosingMessage : IInternalMessage
    {
        private int _numberOfClosingCallbacksCalled = 0;
        public bool CloneMessage => false;

        public int TotalNumberOfClosingCallbacks { get; init; }

        public int NumberOfClosingCallbacksCalled
        {
            get => _numberOfClosingCallbacksCalled;
            set
            {
                _numberOfClosingCallbacksCalled = value;
                CallbackOnClosingCallbackCalled?.Invoke();
            }
        }

        public Action? CallbackOnClosingCallbackCalled { get; set; }

        public object Clone()
            => throw new NotImplementedException();
    }
}
