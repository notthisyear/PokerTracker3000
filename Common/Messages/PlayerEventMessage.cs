using PokerTracker3000.GameComponents;
using PokerTracker3000.Interfaces;

namespace PokerTracker3000.Common.Messages
{
    public class PlayerEventMessage(PlayerEventMessage.Type messageType, string playerName, decimal addOnOrBuyInAmount, decimal playerTotal, decimal potTotal, CurrencyType currency) : IInternalMessage
    {
        public enum Type
        {
            BuyIn,
            AddOn,
            Eliminated,
        }

        public Type MessageType { get; } = messageType;

        public string PlayerName { get; } = playerName;

        public decimal AddOnOrBuyInAmount { get; } = addOnOrBuyInAmount;

        public decimal PlayerTotal { get; } = playerTotal;

        public decimal PotTotal { get; } = potTotal;

        public CurrencyType Currency { get; } = currency;

        public bool CloneMessage => true;

        public object Clone()
            => new PlayerEventMessage(MessageType, PlayerName, AddOnOrBuyInAmount, PlayerTotal, PotTotal, Currency);
    }
}
