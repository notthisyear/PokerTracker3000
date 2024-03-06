using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Ookii.Dialogs.Wpf;

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
        private bool _isEliminated = false;
        private static readonly VistaOpenFileDialog s_loadImageDialog = new()
        {
            Title = "Select player image",
            Multiselect = false,
            Filter = "Image files (*.png, *.jpg, *.jpeg, *.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*"
        };
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

        public bool IsEliminated
        {
            get => _isEliminated;
            set
            {
                if (value != _isEliminated)
                {
                    if (value)
                    {
                        SpotOptions.First(x => x.Option == PlayerEditOption.EditOption.Eliminate).ChangeEditOption(PlayerEditOption.EditOption.Remove);
                        SpotOptions.First(x => x.Option == PlayerEditOption.EditOption.AddOn).ChangeEditOption(PlayerEditOption.EditOption.BuyIn);
                    }
                    else
                    {
                        SpotOptions.First(x => x.Option == PlayerEditOption.EditOption.Remove).ChangeEditOption(PlayerEditOption.EditOption.Eliminate);
                        SpotOptions.First(x => x.Option == PlayerEditOption.EditOption.BuyIn).ChangeEditOption(PlayerEditOption.EditOption.AddOn);
                    }
                }
                SetProperty(ref _isEliminated, value);
            }
        }

        public int SpotIndex { get; init; }

        public bool HasPlayerData { get => _playerData != null; }
        #endregion

        public PlayerSpot()
        {
            SpotOptions = new()
            {
                new(PlayerEditOption.EditOption.ChangeName, isSelected: true),
                new(PlayerEditOption.EditOption.ChangeImage),
                new(PlayerEditOption.EditOption.AddOn, PlayerEditOption.OptionType.Success),
                new(PlayerEditOption.EditOption.Eliminate, PlayerEditOption.OptionType.Cancel),
            };
        }

        #region Public methods
        public void ChangeSelectedOption(InputEvent.NavigationDirection direction)
        {
            var currentOption = GetSelectedOption();
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

        public PlayerEditOption GetSelectedOption()
            => SpotOptions.First(x => x.IsSelected);

        public bool IsPlayer(int id)
            => PlayerData != default && PlayerData.PlayerId == id;

        public void ChangeImage()
        {
            if (PlayerData != default && s_loadImageDialog.ShowDialog() == true)
            {
                // TODO: Make a nice image loader dialog that supports cropping the selected image
                PlayerData.Information.PathToImage = s_loadImageDialog.FileName;
            }
        }
        #endregion
    }
}
