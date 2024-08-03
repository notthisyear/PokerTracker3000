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
        #endregion

        public static PlayerModel GetNewPlayer(string pathToImage)
            => new("<NEW PLAYER>", pathToImage);

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

        [JsonConstructor]
        private PlayerModel(string name, string pathToImage, decimal moneyInThePot = decimal.Zero)
        {
            Name = name;
            PathToImage = pathToImage;
            MoneyInThePot = moneyInThePot;
        }
    }
}
