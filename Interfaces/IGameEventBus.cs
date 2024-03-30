using System;
using System.Collections.Generic;
using static PokerTracker3000.Common.GameEventBus;

namespace PokerTracker3000.Interfaces
{
    public interface IGameEventBus
    {
        public void NotifyListeners(EventType messageType, IInternalMessage message);

        public void RegisterListener(object registrator, Action<EventType, IInternalMessage> callbackType, EventType listenTo, bool getMostRecentMessage = false);

        public void RegisterListener(object registrator, Action<EventType, IInternalMessage> callbackType, List<EventType> listenTo, bool getMostRecentMessage = false);

        public void DeregisterListener(object registrator);
    }
}
