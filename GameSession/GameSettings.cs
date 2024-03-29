using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.GameComponents;

namespace PokerTracker3000.GameSession
{
    public class GameSettings : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private CurrencyType _currencyType = CurrencyType.SwedishKrona;
        private decimal _defaultBuyInAmount = 500;
        private decimal _defaultAddOnAmount = 500;
        private int _defaultStageLengthSeconds = 20 * 60;
        #endregion

        public CurrencyType CurrencyType
        {
            get => _currencyType;
            set => SetProperty(ref _currencyType, value);
        }

        public decimal DefaultBuyInAmount
        {
            get => _defaultBuyInAmount;
            set => SetProperty(ref _defaultBuyInAmount, value);
        }

        public decimal DefaultAddOnAmount
        {
            get => _defaultAddOnAmount;
            set => SetProperty(ref _defaultAddOnAmount, value);
        }

        public int DefaultStageLengthSeconds
        {
            get => _defaultStageLengthSeconds;
            set => SetProperty(ref _defaultStageLengthSeconds, value);
        }
        #endregion
    }
}
