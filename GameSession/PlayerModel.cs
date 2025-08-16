using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using PokerTracker3000.Common;
using PokerTracker3000.Common.FileUtilities;

namespace PokerTracker3000.GameSession
{
    public class PlayerModel : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private string _name = string.Empty;
        private string _pathToImage = string.Empty;
        private decimal _moneyInThePot = decimal.Zero;
        private bool _hasData = false;
        private bool _isChipLead = false;
        #endregion

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string PathToImage
        {
            get => _pathToImage;
            set => SetProperty(ref _pathToImage, value);
        }

        public decimal MoneyInThePot
        {
            get => _moneyInThePot;
            set => SetProperty(ref _moneyInThePot, value);
        }

        public bool HasData
        {
            get => _hasData;
            private set => SetProperty(ref _hasData, value);
        }

        public bool IsChipLead
        {
            get => _isChipLead;
            set => SetProperty(ref _isChipLead, value);
        }
        #endregion

        public PlayerModel() : this(string.Empty, string.Empty)
        {
            HasData = false;
        }

        [JsonConstructor]
        public PlayerModel(string name, string pathToImage, decimal moneyInThePot = decimal.Zero)
        {
            Set(name, pathToImage, moneyInThePot);
        }

        public static bool TryLoadPlayerFromFile(string pathToFile, out PlayerModel? player)
        {
            player = default;
            FileTextReader reader = new(pathToFile);
            if (!reader.SuccessfulRead)
                return false;

            var (playerModel, _) = reader.AllText.DeserializeJsonString<PlayerModel>(convertSnakeCaseToPascalCase: true);
            if (playerModel != default && !string.IsNullOrEmpty(playerModel.Name))
            {
                player = playerModel!;
                return true;
            }
            return false;
        }

        public void Set(PlayerModel model)
        {
            Set(model.Name, model.PathToImage, model.MoneyInThePot);
        }

        public void Clear()
        {
            Name = string.Empty;
            PathToImage = string.Empty;
            MoneyInThePot = decimal.Zero;
            IsChipLead = false;
            HasData = false;
        }

        public void Set(string name, string pathToImage, decimal moneyInThePot)
        {
            Name = name;
            PathToImage = pathToImage;
            MoneyInThePot = moneyInThePot;
            HasData = true;
        }
    }
}
