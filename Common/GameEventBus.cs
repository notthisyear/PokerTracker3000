using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using PokerTracker3000.Interfaces;

using MessageCallbackType = System.Action<PokerTracker3000.Common.GameEventBus.EventType, PokerTracker3000.Interfaces.IInternalMessage>;

namespace PokerTracker3000.Common
{
    public class GameEventBus : IGameEventBus
    {
        public enum EventType
        {
            [Description("Game started")]
            /// <summary>
            /// Game started message.
            /// Expects an instance of <see cref="Messages.GameEventMessage"/> as its message data.
            /// </summary>
            GameStarted,

            [Description("Game paused")]
            /// <summary>
            /// Game paused message.
            /// Expects an instance of <see cref="Messages.GameEventMessage"/> as its message data.
            /// </summary>
            GamePaused,

            [Description("Game done")]
            /// <summary>
            /// Game done message.
            /// Expects an instance of <see cref="Messages.GameEventMessage"/> as its message data.
            /// </summary>
            GameDone,

            [Description("Stage changed")]
            /// <summary>
            /// Staged changed message.
            /// Expects an instance of <see cref="Messages.StageChangedMessage"/> as its message data.
            /// </summary>
            StageChanged,

            [Description("Player eliminated")]
            /// <summary>
            /// Player eliminated message.
            /// Expects an instance of <see cref="Messages.PlayerEventMessage"/> as its message data.
            /// </summary>
            PlayerEliminated,

            [Description("Player add-on")]
            /// <summary>
            /// Player add-on message.
            /// Expects an instance of <see cref="Messages.PlayerEventMessage"/> as its message data.
            /// </summary>
            PlayerAddOn,

            [Description("Player buy-in")]
            /// <summary>
            /// Player add-on message.
            /// Expects an instance of <see cref="Messages.PlayerEventMessage"/> as its message data.
            /// </summary>
            PlayerBuyIn,

            [Description("New chip leader")]
            /// <summary>
            /// A new chip lead message.
            /// Expects an instance of <see cref="Messages.PlayerEventMessage"/> as its message data.
            /// </summary>
            NewChipLead,

            /// <summary>
            /// A request to either start or stop a time event generation at a particular remaining stage time.
            /// Expects an instance of <see cref="Messages.StageTimeRemainingEventRequestMessage"/> as its message data.
            /// </summary>
            StageTimeRemainingEventRequest,

            [Description("Time event")]
            /// <summary>
            /// A stage time remaining event. Expects an instance of <see cref="Messages.StageTimeRemainingEventMessage"/> as its message data.
            /// </summary>
            StageTimeRemainingEvent,

            [Description("Application closing")]
            /// <summary>
            /// The application is closing. Expects an instance of <see cref="Messages.ApplicationClosingMessage"/> as its message data.
            /// </summary>
            ApplicationClosing
        }

        #region Private fields
        private readonly Dictionary<EventType, IInternalMessage> _lastMessage = [];

        private readonly Dictionary<EventType, List<(long registratorId, MessageCallbackType callback)>> _listeners = [];
        private readonly Dictionary<long, object> _registrators = [];
        private readonly ConcurrentQueue<(EventType type, IInternalMessage message)> _internalMessagesQueue = new();
        private readonly int _workerThreadId;
        private long _registratorId = 0;
        #endregion

        public GameEventBus()
        {
            var t = new Thread(() => MonitorInternalMessageQueue()) { IsBackground = true };
            _workerThreadId = t.ManagedThreadId;
            t.Start();
        }

        public void NotifyListeners(EventType type, IInternalMessage message)
        {
            if (message != null)
                _internalMessagesQueue.Enqueue((type, message));
        }

        public void RegisterListener(object registrator, MessageCallbackType callbackType, EventType listenTo, bool getMostRecentMessage = false)
            => RegisterListener(registrator, callbackType, [listenTo], getMostRecentMessage);

        public void RegisterListener(object registrator, MessageCallbackType callbackType, List<EventType> listenTo, bool getMostRecentMessage = false)
        {
            var registratorId = GetIdForRegistrator(registrator);
            lock (_listeners)
            {
                foreach (var messageType in listenTo)
                {
                    var messageTypeHasSubscriptions = _listeners.TryGetValue(messageType, out var registratorIds);
                    if (messageTypeHasSubscriptions && registratorIds!.Select(x => x.registratorId).Contains(registratorId))
                        throw new InvalidOperationException($"Caller is already registered for the message type '{messageType}'");

                    if (messageTypeHasSubscriptions)
                        registratorIds!.Add((registratorId, callbackType));
                    else
                        _listeners.Add(messageType, [(registratorId, callbackType)]);
                }
            }

            if (getMostRecentMessage)
            {
                foreach (var type in listenTo)
                {
                    if (_lastMessage.TryGetValue(type, out var value))
                        callbackType(type, value);
                }
            }
        }

        public int GetNumberOfListenersForEvent(EventType eventType)
        {
            var result = 0;
            lock (_listeners)
            {
                if (_listeners.TryGetValue(eventType, out var listeners))
                    result = listeners.Count;

            }
            return result;
        }

        public void DeregisterListener(object registrator)
        {
            if (Environment.CurrentManagedThreadId == _workerThreadId)
                throw new InvalidOperationException("Cannot deregister listener from the worker thread");

            var registratorId = GetIdForRegistrator(registrator);
            lock (_listeners)
            {
                foreach (var entry in _listeners)
                {
                    var matches = entry.Value.Where(x => x.registratorId == registratorId);
                    if (matches.Count() == 1)
                        entry.Value.Remove(matches.First());
                }
            }
            RemoveRegisteredId(registratorId);
        }

        private void MonitorInternalMessageQueue()
        {
            while (true)
            {
                if (_internalMessagesQueue.TryDequeue(out var item))
                    PropagateToListeners(item.type, item.message);
                else
                    Thread.Sleep(50);
            }
        }

        private void PropagateToListeners(EventType type, IInternalMessage message)
        {
            lock (_listeners)
            {
                if (_listeners.TryGetValue(type, out var callbacks))
                {
                    foreach (var (registratorId, callback) in callbacks)
                    {
                        var messageToPropagate = message.CloneMessage ? (IInternalMessage)message.Clone() : message;
                        callback(type, messageToPropagate);
                    }
                }
            }

            if (message.CloneMessage)
            {
                if (_lastMessage.ContainsKey(type))
                    _lastMessage[type] = (IInternalMessage)message.Clone();
                else
                    _lastMessage.Add(type, (IInternalMessage)message.Clone());
            }
        }

        private long GetIdForRegistrator(object registrator)
        {
            long registratorId;
            lock (_registrators)
            {
                if (_registrators.Values.Where(x => ReferenceEquals(x, registrator)).Any())
                {
                    registratorId = _registrators.FirstOrDefault(x => ReferenceEquals(x.Value, registrator)).Key;
                }
                else
                {
                    registratorId = _registratorId++;
                    _registrators.Add(registratorId, registrator);
                }
            }
            return registratorId;
        }

        private void RemoveRegisteredId(long registratorId)
        {
            lock (_registrators)
                _registrators.Remove(registratorId);
        }
    }
}


