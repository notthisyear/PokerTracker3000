using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.GameComponents;

using InputEvent = PokerTracker3000.Input.InputManager.UserInputEvent;

namespace PokerTracker3000.GameSession
{
    public enum TableLayout
    {
        TwoPlayers,
        FourPlayers,
        SixPlayers,
        EightPlayers,
        TenPlayers,
        TwelvePlayers
    };

    public class GameSessionManager : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private PlayerSpot? _selectedSpot;
        private CurrencyType _currencyType = CurrencyType.SwedishKrona;
        private bool _tableFull = false;
        #endregion

        public List<PlayerSpot> PlayerSpots { get; } = [];

        public PlayerSpot? SelectedSpot
        {
            get => _selectedSpot;
            private set => SetProperty(ref _selectedSpot, value);
        }

        public CurrencyType CurrencyType
        {
            get => _currencyType;
            private set => SetProperty(ref _currencyType, value);
        }

        public bool TableFull
        {
            get => _tableFull;
            private set => SetProperty(ref _tableFull, value);
        }

        public GameClock Clock { get; } = new();

        public MainWindowFocusManager FocusManager { get; }
        #endregion

        #region Events
        public event EventHandler<int>? PlayerSpotsUpdatedEvent;
        #endregion

        #region Private fields
        private const int NumberOfPlayerSpots = 12;
        private readonly string _pathToDefaultPlayerImage;
        private readonly NavigationManager _navigationManager;
        private int _nextPlayerId = 0;
        private bool _moveInProgress = false;
        private TableLayout _currentTableLayout;
        #endregion

        public GameSessionManager(string pathToDefaultPlayerImage, MainWindowFocusManager focusManager)
        {
            FocusManager = focusManager;

            _pathToDefaultPlayerImage = pathToDefaultPlayerImage;
            for (var i = 0; i < NumberOfPlayerSpots; i++)
                PlayerSpots.Add(new() { SpotIndex = i });

            _navigationManager = new(PlayerSpots.AsReadOnly());

            RegisterFocusManagerCallbacks();
            InitializeSpots(8);
        }

        public void SetTableLayout(TableLayout tableLayout)
        {
            _currentTableLayout = tableLayout;
        }

        public void AddPlayerToSpot()
        {
            var targetSpot = PlayerSpots.FirstOrDefault(x => !x.HasPlayerData);
            if (targetSpot == default)
                return;

            targetSpot.AddPlayer(_nextPlayerId++, _pathToDefaultPlayerImage);
            PlayerSpotsUpdatedEvent?.Invoke(this, NumberOfPlayerSpots);
            TableFull = !PlayerSpots.Where(x => !x.HasPlayerData).Any();
        }

        #region Private methods
        private void InitializeSpots(int numberOfSpots)
        {
            for (var i = 0; i < numberOfSpots; i++)
                PlayerSpots[i].AddPlayer(_nextPlayerId++, _pathToDefaultPlayerImage);
        }

        private void RegisterFocusManagerCallbacks()
        {
            FocusManager.RegisterPlayerSpots(PlayerSpots);
            FocusManager.RegisterSpotNavigationCallback((int currentSpotIdx, InputEvent.NavigationDirection direction) =>
            {
                // Note: The navigation is set-up in such a way that if no
                //       available spot is found in the requested navigation
                //       direction, the current spot index is returned
                var newSpotIndex = _navigationManager.Navigate(_currentTableLayout, currentSpotIdx, direction, _moveInProgress);
                if (_moveInProgress)
                {
                    var currentSpot = PlayerSpots.First(x => x.SpotIndex == currentSpotIdx);
                    PlayerSpots.First(x => x.SpotIndex == newSpotIndex).Swap(currentSpot);
                }
                return newSpotIndex;
            });
            FocusManager.RegisterPlayerOptionsCallback((PlayerSpot activeSpot, InputEvent.NavigationDirection direction) => activeSpot.ChangeSelectedOption(direction));
            FocusManager.RegisterPlayerInfoBoxSelectCallback((PlayerSpot activeSpot) =>
            {
                switch (activeSpot.GetSelectedOption().Option)
                {
                    case PlayerEditOption.EditOption.ChangeName:
                        SelectedSpot = activeSpot;
                        return MainWindowFocusManager.FocusArea.EditNameBox;

                    case PlayerEditOption.EditOption.ChangeImage:
                        activeSpot.ChangeImage();
                        // TODO: When there's a nice image picker dialog, this line
                        //       will have to change
                        return MainWindowFocusManager.FocusArea.PlayerInfo;

                    case PlayerEditOption.EditOption.Eliminate:
                        activeSpot.IsEliminated = true;
                        return MainWindowFocusManager.FocusArea.PlayerInfo;

                    case PlayerEditOption.EditOption.Remove:
                        if (!activeSpot.CanBeRemoved)
                            return MainWindowFocusManager.FocusArea.PlayerInfo;

                        activeSpot.RemovePlayer();
                        var newSpotIdx = activeSpot.SpotIndex + 1;
                        var newSpotToFocus = activeSpot;
                        while (!newSpotToFocus.HasPlayerData)
                            newSpotToFocus = PlayerSpots[newSpotIdx++ % NumberOfPlayerSpots];
                        newSpotToFocus.IsSelected = true;

                        if (TableFull)
                            TableFull = false;

                        return MainWindowFocusManager.FocusArea.Players;

                    case PlayerEditOption.EditOption.Move:
                        _moveInProgress = true;
                        activeSpot.IsBeingMoved = true;
                        return MainWindowFocusManager.FocusArea.MovementInProgress;

                    case PlayerEditOption.EditOption.AddOn:
                    case PlayerEditOption.EditOption.BuyIn:

                    default:
                        return MainWindowFocusManager.FocusArea.None;
                };
            });
            FocusManager.RegisterEditMenuLostFocusCallback(() =>
            {
                SelectedSpot = default;
            });
            FocusManager.RegisterMovementDoneCallback((PlayerSpot spot) =>
            {
                _moveInProgress = false;
                spot.IsBeingMoved = false;
            });
        }
        #endregion
    }
}
