using System.Collections.ObjectModel;
using System.Windows.Data;

namespace PokerTracker3000.GameSession
{
    public class ChipManager
    {
        #region Public properties
        public ObservableCollection<ChipModel> Chips { get; }
        #endregion

        #region Private fields
        private int _nextChipId = 0;
        private readonly object _chipsAccessLock = new();
        #endregion

        public ChipManager()
        {
            Chips = [];
            BindingOperations.EnableCollectionSynchronization(Chips, _chipsAccessLock);
        }

        #region Public methods
        public void AddChip(string mainColorHexString = "", string accentColorHexString = "", decimal value = decimal.MinValue)
        {
            lock (_chipsAccessLock)
            {
                var newChip = new ChipModel() { Id = _nextChipId++ };
                newChip.MainColorHexString = string.IsNullOrEmpty(mainColorHexString) ? newChip.MainColorHexString : mainColorHexString;
                newChip.AccentColorHexString = string.IsNullOrEmpty(accentColorHexString) ? newChip.AccentColorHexString : accentColorHexString;
                newChip.Value = value == decimal.MinValue ? newChip.Value : value;
                Chips.Add(newChip);
            }
        }
        #endregion
    }
}
