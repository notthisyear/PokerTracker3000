using CommunityToolkit.Mvvm.ComponentModel;

namespace PokerTracker3000.Common
{
    public class SelectableEntity : ObservableObject
    {
        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }
    }
}
