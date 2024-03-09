using System;
using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.Common;
using PokerTracker3000.Common.FileUtilities;

namespace PokerTracker3000.GameSession
{
    public class PlayerModel
    {
        public class PlayerInformation : ObservableObject
        {
            #region Public propertoes

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

            public PlayerInformation(string name, string pathToImage, decimal moneyInThePot = decimal.Zero)
            {
                Name = name;
                PathToImage = pathToImage;
                MoneyInThePot = moneyInThePot;
            }
        }

        #region Public properties
        public int PlayerId { get; }

        public PlayerInformation Information { get; }
        #endregion

        public static PlayerModel GetNewPlayer(int playerId, string pathToImage)
            => new(playerId, new("<NEW PLAYER>", pathToImage));

        public static PlayerModel GetPlayer(string pathToPlayerFile)
            => throw new NotImplementedException();

        public static Exception? TryLoadPlayerFromFile(string pathToFile, int playerId, out PlayerModel? player)
        {
            player = default;
            FileTextReader reader = new(pathToFile);
            if (!reader.SuccessfulRead)
                return reader.ReadException!;

            var (playerInformation, e) = reader.AllText.DeserializeJsonString<PlayerInformation>(convertSnakeCaseToPascalCase: true);
            if (e != default)
                player = new(playerId, playerInformation!);

            return e;
        }

        private PlayerModel(int playerId, PlayerInformation information)
        {
            PlayerId = playerId;
            Information = information;
        }
    }
}
