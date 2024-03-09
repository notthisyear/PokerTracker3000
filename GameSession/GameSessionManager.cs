using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace PokerTracker3000.GameSession
{
    public class GameSessionManager
    {
        public ObservableCollection<PlayerModel> Players { get; } = new();

        public GameClock Clock { get; } = new();

        private ICommand? _addPlayerCommand = default;
        private ICommand? _removePlayerCommand = default;

        public ICommand AddPlayerCommand
        {
            get
            {
                _addPlayerCommand ??= new RelayCommand(() =>
                {
                    lock (_playersLock)
                        Players.Add(PlayerModel.GetNewPlayer(_nextPlayerId++, _pathToDefaultPlayerImage));
                });
                return _addPlayerCommand;
            }
        }

        public ICommand RemovePlayerCommand
        {
            get
            {
                _removePlayerCommand ??= new RelayCommand<PlayerModel>(p =>
                {
                    if (p == default)
                        return;

                    lock (_playersLock)
                    {
                        var playerToRemove = Players.FirstOrDefault(x => x.PlayerId == p.PlayerId);
                        if (playerToRemove != default)
                            Players.Remove(playerToRemove);
                    }
                });
                return _removePlayerCommand;
            }
        }


        private readonly string _pathToDefaultPlayerImage;
        private readonly object _playersLock = new();
        private int _nextPlayerId = 0;

        public GameSessionManager(string pathToDefaultPlayerImage)
        {
            BindingOperations.EnableCollectionSynchronization(Players, _playersLock);
            _pathToDefaultPlayerImage = pathToDefaultPlayerImage;

            lock (_playersLock)
            {
                Players.Add(PlayerModel.GetNewPlayer(_nextPlayerId++, _pathToDefaultPlayerImage));
                Players.Add(PlayerModel.GetNewPlayer(_nextPlayerId++, _pathToDefaultPlayerImage));
                Players.Add(PlayerModel.GetNewPlayer(_nextPlayerId++, _pathToDefaultPlayerImage));
                Players.Add(PlayerModel.GetNewPlayer(_nextPlayerId++, _pathToDefaultPlayerImage));
                Players.Add(PlayerModel.GetNewPlayer(_nextPlayerId++, _pathToDefaultPlayerImage));
                Players.Add(PlayerModel.GetNewPlayer(_nextPlayerId++, _pathToDefaultPlayerImage));
                Players.Add(PlayerModel.GetNewPlayer(_nextPlayerId++, _pathToDefaultPlayerImage));
                Players.Add(PlayerModel.GetNewPlayer(_nextPlayerId++, _pathToDefaultPlayerImage));

            }
        }
    }
}
