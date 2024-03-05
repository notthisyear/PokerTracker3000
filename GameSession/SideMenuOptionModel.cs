using CommunityToolkit.Mvvm.ComponentModel;

namespace PokerTracker3000.GameSession
{
    public class SideMenuOptionModel : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private string _optionText = string.Empty;
        private bool _isHighlighted = true;
        private bool _isSubOption = false;
        #endregion
        public string OptionText
        {
            get => _optionText;
            set => SetProperty(ref _optionText, value);
        }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set => SetProperty(ref _isHighlighted, value);
        }

        public bool IsSubOption
        {
            get => _isSubOption;
            set => SetProperty(ref _isSubOption, value);
        }
        #endregion
    }
}
