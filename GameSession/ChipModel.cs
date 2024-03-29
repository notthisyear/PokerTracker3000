using CommunityToolkit.Mvvm.ComponentModel;

namespace PokerTracker3000.GameSession
{
    public class ChipModel : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private string _mainColorHexString = "#FFF8F8FF";
        private string _accentColorHexString = "#FF000000";
        private decimal _value = 1;
        #endregion

        public string MainColorHexString
        {
            get => _mainColorHexString;
            set => SetProperty(ref _mainColorHexString, value);
        }

        public string AccentColorHexString
        {
            get => _accentColorHexString;
            set => SetProperty(ref _accentColorHexString, value);
        }

        public decimal Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public int Id { get; init; }
        #endregion
    }
}
