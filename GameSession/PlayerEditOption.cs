using CommunityToolkit.Mvvm.ComponentModel;

namespace PokerTracker3000.GameSession
{
    public class PlayerEditOption : ObservableObject
    {
        private bool _isSelected = false;

        public enum OptionType
        {
            Default,
            Success,
            Cancel
        };

        public string Name { get; init; } = string.Empty;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public OptionType Type { get; init; } = OptionType.Default;
    }
}
