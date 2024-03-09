using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

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
        #endregion
        public List<PlayerSpot> PlayerSpots { get; } = new();

        public int NumberOfActivePlayers { get; private set; } = 0;

        public PlayerSpot? SelectedSpot
        {
            get => _selectedSpot;
            private set => SetProperty(ref _selectedSpot, value);
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
        private int _nextPlayerId = 0;

        private TableLayout _currentTableLayout;
        #endregion

        public GameSessionManager(string pathToDefaultPlayerImage, MainWindowFocusManager focusManager)
        {
            _pathToDefaultPlayerImage = pathToDefaultPlayerImage;
            FocusManager = focusManager;

            for (var i = 0; i < NumberOfPlayerSpots; i++)
                PlayerSpots.Add(new() { SpotIndex = i });

            RegisterFocusManagerCallbacks();
            InitializeSpots(5);
        }

        public void SetTableLayout(TableLayout tableLayout)
        {
            _currentTableLayout = tableLayout;
        }

        private void RegisterFocusManagerCallbacks()
        {
            FocusManager.RegisterPlayerSpots(PlayerSpots);
            FocusManager.RegisterSpotNavigationCallback((int currentSpotIdx, InputEvent.NavigationDirection direction) =>
            {
                (MainWindowFocusManager.NavigationCallback navigationCallback,
                Func<int, InputEvent.NavigationDirection, MainWindowFocusManager.NavigationCallback, int>? navigationFailureHandler) navigationResult
                = _currentTableLayout switch
                {
                    TableLayout.TwoPlayers => (NavigateTwoPlayer, NavigateTheSameDirectionAgain),
                    TableLayout.FourPlayers => (NavigateFourPlayer, NavigateTheSameDirectionAgain),
                    TableLayout.SixPlayers => (NavigateSixPlayer, NavigateTheSameDirectionAgain),
                    TableLayout.EightPlayers => (NavigateEightPlayers, default),
                    _ => ((i, d) => i, default)
                };

                var newSpotIndex = navigationResult.navigationCallback(currentSpotIdx, direction);
                // Note: The failure handler should be set-up in such a way that we'll
                //       eventually come back to where we started, meaning that no valid
                //       navigation for the current direction exists.
                while (!PlayerSpots[newSpotIndex].HasPlayerData && navigationResult.navigationFailureHandler != default)
                    newSpotIndex = navigationResult.navigationFailureHandler(newSpotIndex, direction, navigationResult.navigationCallback);
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
                        return MainWindowFocusManager.FocusArea.Players;

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
        }

        private void InitializeSpots(int numberOfSpots)
        {
            for (var i = 0; i < numberOfSpots; i++)
                PlayerSpots[i].AddPlayer(_nextPlayerId++, _pathToDefaultPlayerImage);
            NumberOfActivePlayers = numberOfSpots;
        }

        private static int NavigateTwoPlayer(int currentSpotIdx, InputEvent.NavigationDirection direction)
            => currentSpotIdx == 0 ? 1 : 0;

        private static int NavigateFourPlayer(int currentSpotIdx, InputEvent.NavigationDirection direction)
        {
            var onTopRow = currentSpotIdx == 0 || currentSpotIdx == 1;
            return direction switch
            {
                InputEvent.NavigationDirection.Right or InputEvent.NavigationDirection.Left => ((currentSpotIdx + 1) % 2) + (onTopRow ? 0 : 2),
                InputEvent.NavigationDirection.Up or InputEvent.NavigationDirection.Down => (currentSpotIdx + 2) % 4,
                _ => currentSpotIdx
            };
        }

        private static int NavigateSixPlayer(int currentSpotIdx, InputEvent.NavigationDirection direction)
        {
            var onTopRow = currentSpotIdx == 0 || currentSpotIdx == 1 || currentSpotIdx == 2;
            return direction switch
            {
                InputEvent.NavigationDirection.Right => ((currentSpotIdx + 1) % 3) + (onTopRow ? 0 : 3),
                InputEvent.NavigationDirection.Left => onTopRow ? (currentSpotIdx == 0 ? 2 : currentSpotIdx - 1) : (currentSpotIdx == 3 ? 5 : currentSpotIdx - 1),
                InputEvent.NavigationDirection.Up or InputEvent.NavigationDirection.Down => (currentSpotIdx + 3) % 6,
                _ => currentSpotIdx
            };
        }

        private static int NavigateEightPlayers(int currentSpotIdx, InputEvent.NavigationDirection direction)
        {
            switch (direction)
            {
                case InputEvent.NavigationDirection.Right:
                    return currentSpotIdx switch
                    {
                        < 3 => currentSpotIdx + 1,
                        3 => 7,
                        > 3 => currentSpotIdx - 1
                    };
                case InputEvent.NavigationDirection.Left:
                    return currentSpotIdx switch
                    {
                        0 => 7,
                        < 4 => currentSpotIdx - 1,
                        >= 4 and < 7 => currentSpotIdx + 1,
                        7 => 3,
                        _ => currentSpotIdx
                    };
                case InputEvent.NavigationDirection.Up:
                    return currentSpotIdx switch
                    {
                        (>= 0 and < 3) or 5 => 6 - currentSpotIdx,
                        3 or 4 => currentSpotIdx - 1,
                        6 or 7 => (currentSpotIdx + 1) % 8,
                        _ => currentSpotIdx
                    };
                case InputEvent.NavigationDirection.Down:
                    return currentSpotIdx switch
                    {
                        0 => 7,
                        7 => 6,
                        (>= 4 and <= 6) or 1 => 6 - currentSpotIdx,
                        2 or 3 => currentSpotIdx + 1,
                        _ => currentSpotIdx
                    };
            }
            return currentSpotIdx;
        }

        private int NavigateTheSameDirectionAgain(int currentSpotIdx, InputEvent.NavigationDirection direction, MainWindowFocusManager.NavigationCallback callback)
            => callback(currentSpotIdx, direction);
    }
}
