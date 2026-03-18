using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace PokerTracker3000.Common
{
    public class SelectableEntity : ObservableObject
    {
        private bool _isSelected = false;

        [JsonIgnore]
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }
    }
}
