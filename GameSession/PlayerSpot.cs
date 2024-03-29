using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Ookii.Dialogs.Wpf;
using PokerTracker3000.Common;

namespace PokerTracker3000.GameSession
{
    public class PlayerSpot : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private PlayerModel? _playerData = default;
        private bool _isHighlighted = false;
        private bool _isSelected = false;
        private bool _isBeingMoved = false;
        private bool _isEliminated = false;
        private decimal _buyInOrAddOnAmount = 0;
        private static readonly VistaOpenFileDialog s_loadImageDialog = new()
        {
            Title = "Select player image",
            Multiselect = false,
            Filter = "Image files (*.png, *.jpg, *.jpeg, *.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*"
        };
        private static readonly VistaSaveFileDialog s_savePlayerDialog = new()
        {
            Title = "Select file name",
            AddExtension = true,
            DefaultExt = "json",
            Filter = "JSON file (*.json)|*.json"
        };
        #endregion

        public PlayerModel? PlayerData
        {
            get => _playerData;
            private set => SetProperty(ref _playerData, value);
        }

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

        public bool IsBeingMoved
        {
            get => _isBeingMoved;
            set => SetProperty(ref _isBeingMoved, value);
        }

        public bool IsEliminated
        {
            get => _isEliminated;
            set => SetProperty(ref _isEliminated, value);
        }

        public decimal BuyInOrAddOnAmount
        {
            get => _buyInOrAddOnAmount;
            set => SetProperty(ref _buyInOrAddOnAmount, value);
        }

        public int SpotIndex { get; init; }

        public bool HasPlayerData { get => _playerData != null; }
        #endregion

        #region Public methods
        public void AddPlayer(int playerId, string pathToImage)
        {
            PlayerData = PlayerModel.GetNewPlayer(playerId, pathToImage);
        }

        public void AddPlayer(string pathToPlayerFile)
        {
            PlayerData = PlayerModel.GetPlayer(pathToPlayerFile);
        }

        public void RemovePlayer()
        {
            IsHighlighted = false;
            IsSelected = false;
            IsEliminated = false;
            PlayerData = default;
        }

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

        public void SavePlayer()
        {
            if (PlayerData != default && s_savePlayerDialog.ShowDialog() == true)
            {
                var (s, e) = PlayerData.Information.SerializeToJsonString(convertPascalCaseToSnakeCase: true, indent: true);
                if (e != default)
                    return;

                var path = Path.ChangeExtension(s_savePlayerDialog.FileName, s_savePlayerDialog.DefaultExt);
                _ = new FileTextWriter(s!, path);
            }
        }

        public void Swap(PlayerSpot other, bool moveInAction = true)
        {
            var thisPlayerData = PlayerData;
            var thisIsEliminated = IsEliminated;
            PlayerData = other.PlayerData;
            IsEliminated = other.IsEliminated;
            IsBeingMoved = moveInAction;

            other.PlayerData = thisPlayerData;
            other.IsEliminated = thisIsEliminated;
            other.IsBeingMoved = false;
        }
        #endregion
    }
}
