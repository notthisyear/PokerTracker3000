using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

using InputEvent = PokerTracker3000.Input.InputManager.UserInputEvent;

namespace PokerTracker3000.GameSession
{
    public class PlayerSpot : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private PlayerModel? _playerData = default;
        private bool _isHighlighted = false;
        private bool _isSelected = false;
        #endregion

        public PlayerModel? PlayerData
        {
            get => _playerData;
            set => SetProperty(ref _playerData, value);
        }

        public List<PlayerEditOption> SpotOptions { get; init; }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set => SetProperty(ref _isHighlighted, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public int SpotIndex { get; init; }

        public bool HasPlayerData { get => _playerData != null; }

        public PlayerSpot()
        {
            SpotOptions = new()
            {
                new() { Name = "Change name", IsSelected = true },
                new() { Name = "Change image" },
                new() { Name = "Buy-in", Type = PlayerEditOption.OptionType.Success },
                new() { Name = "Remove", Type = PlayerEditOption.OptionType.Cancel },
            };
        }

        public void ChangeSelectedOption(InputEvent.NavigationDirection direction)
        {
            var currentOption = SpotOptions.First(x => x.IsSelected);
            var currentOptionIndex = SpotOptions.IndexOf(currentOption);

            var onTopRow = currentOptionIndex < 2;
            var onBottomRow = currentOptionIndex >= (SpotOptions.Count - 2);

            var newOptionIndex = direction switch
            {
                InputEvent.NavigationDirection.Left or InputEvent.NavigationDirection.Right => (currentOptionIndex % 2 == 0) ? currentOptionIndex + 1 : currentOptionIndex - 1,
                InputEvent.NavigationDirection.Down => onBottomRow ? currentOptionIndex % 2 : currentOptionIndex + 2,
                InputEvent.NavigationDirection.Up => onTopRow ? SpotOptions.Count - 2 + currentOptionIndex : currentOptionIndex - 2,
                _ => currentOptionIndex
            };
            currentOption.IsSelected = false;
            SpotOptions[newOptionIndex].IsSelected = true;
        }
        #endregion

        public bool IsPlayer(int id)
            => PlayerData != default && PlayerData.PlayerId == id;
    }
}
