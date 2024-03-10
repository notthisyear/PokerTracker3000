using System;
using System.Collections.Generic;
using System.Linq;
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
        private bool _moveInProgress = false;
        private TableLayout _currentTableLayout;
        #endregion

        public GameSessionManager(string pathToDefaultPlayerImage, MainWindowFocusManager focusManager)
        {
            _pathToDefaultPlayerImage = pathToDefaultPlayerImage;
            FocusManager = focusManager;

            for (var i = 0; i < NumberOfPlayerSpots; i++)
                PlayerSpots.Add(new() { SpotIndex = i });

            RegisterFocusManagerCallbacks();
            InitializeSpots(8);
        }

        public void SetTableLayout(TableLayout tableLayout)
        {
            _currentTableLayout = tableLayout;
        }

        private void InitializeSpots(int numberOfSpots)
        {
            for (var i = 0; i < numberOfSpots; i++)
                PlayerSpots[i].AddPlayer(_nextPlayerId++, _pathToDefaultPlayerImage);
            NumberOfActivePlayers = numberOfSpots;
        }

        #region Private methods
        private void RegisterFocusManagerCallbacks()
        {
            FocusManager.RegisterPlayerSpots(PlayerSpots);
            FocusManager.RegisterSpotNavigationCallback((int currentSpotIdx, InputEvent.NavigationDirection direction) =>
            {
                (MainWindowFocusManager.NavigationCallback navigationCallback,
                Func<int, int, InputEvent.NavigationDirection, MainWindowFocusManager.NavigationCallback, int>? navigationFailureHandler) navigationResult
                = _currentTableLayout switch
                {
                    TableLayout.TwoPlayers => (NavigateTwoPlayer, default),
                    TableLayout.FourPlayers => (NavigateFourPlayer, default),
                    TableLayout.SixPlayers => (NavigateSixPlayer, NavigateTheSameDirectionAgain),
                    TableLayout.EightPlayers => (NavigateEightPlayers, default),
                    _ => ((i, d) => i, default)
                };

                var newSpotIndex = navigationResult.navigationCallback(currentSpotIdx, direction);
                // Note: The failure handler should be set-up in such a way that we'll
                //       eventually come back to where we started, meaning that no valid
                //       navigation for the current direction exists.
                while (!PlayerSpots[newSpotIndex].HasPlayerData && navigationResult.navigationFailureHandler != default)
                    newSpotIndex = navigationResult.navigationFailureHandler(currentSpotIdx, newSpotIndex, direction, navigationResult.navigationCallback);

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

        private int NavigateTwoPlayer(int currentSpotIdx, InputEvent.NavigationDirection _)
            => FindFirstOccupiedSpot(currentSpotIdx, currentSpotIdx + 1 % 2);

        private int NavigateFourPlayer(int currentSpotIdx, InputEvent.NavigationDirection direction)
        {
            // 0:  -> 2, 3 [up, down]     1:  -> 3, 2 [up, down]
            //     -> 1, 3 [left, right]      -> 0, 2 [left, right]
            //
            // 2:  -> 0, 1 [up, down]     3:  -> 0, 0 [up, down]
            //     -> 3, 1 [left, right]      -> 2, 0 [left, right]
            if (direction == InputEvent.NavigationDirection.Up || direction == InputEvent.NavigationDirection.Down)
                return FindFirstOccupiedSpot(currentSpotIdx, (currentSpotIdx + 2) % 4, 3 - currentSpotIdx);
            return FindFirstOccupiedSpot(currentSpotIdx, currentSpotIdx + ((currentSpotIdx % 2 == 0) ? 1 : -1), 3 - currentSpotIdx);
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

        private int NavigateEightPlayers(int currentSpotIdx, InputEvent.NavigationDirection direction)
        {
            var (isUp,
                isDown,
                isLeft,
                isRight) = (direction == InputEvent.NavigationDirection.Up,
                direction == InputEvent.NavigationDirection.Down,
                direction == InputEvent.NavigationDirection.Left,
                direction == InputEvent.NavigationDirection.Right);

            switch (currentSpotIdx)
            {
                case 0:
                    if (isUp || isDown)
                        return FindFirstOccupiedSpot(0, 6, 7);
                    else if (isLeft)
                        return FindFirstOccupiedSpot(0, 7, 3, 2, 4, 1, 5);
                    else if (isRight)
                        return FindFirstOccupiedSpot(0, 1, 5, 2, 4, 3, 7);
                    break;

                case 1:
                    if (isUp || isDown)
                        return FindFirstOccupiedSpot(1, 5);
                    else if (isLeft)
                        return FindFirstOccupiedSpot(1, 0, 6, 7, 3, 2, 4);
                    else if (isRight)
                        return FindFirstOccupiedSpot(1, 2, 4, 3, 7, 0, 6);
                    break;

                case 2:
                    if (isUp || isDown)
                        return FindFirstOccupiedSpot(2, 4, 3);
                    else if (isLeft)
                        return FindFirstOccupiedSpot(2, 1, 5, 0, 6, 7, 3);
                    else if (isRight)
                        return FindFirstOccupiedSpot(2, 3, 7, 0, 6, 1, 5);
                    break;

                case 3:
                    if (isUp)
                        return FindFirstOccupiedSpot(3, 2, 4);
                    else if (isDown)
                        return FindFirstOccupiedSpot(3, 4, 2);
                    else if (isLeft)
                        return FindFirstOccupiedSpot(3, 2, 4, 1, 5, 0, 6, 7);
                    else if (isRight)
                        return FindFirstOccupiedSpot(3, 7, 0, 6, 1, 5, 2, 4);
                    break;

                case 4:
                    if (isUp || isDown)
                        return FindFirstOccupiedSpot(4, 2, 3);
                    else if (isLeft)
                        return FindFirstOccupiedSpot(4, 5, 1, 6, 0, 7, 3);
                    else if (isRight)
                        return FindFirstOccupiedSpot(4, 3, 7, 0, 6, 1, 5);
                    break;

                case 5:
                    if (isUp || isDown)
                        return FindFirstOccupiedSpot(5, 1);
                    else if (isLeft)
                        return FindFirstOccupiedSpot(5, 6, 0, 7, 3, 4, 2);
                    else if (isRight)
                        return FindFirstOccupiedSpot(5, 4, 2, 3, 7, 6, 0);
                    break;

                case 6:
                    if (isUp || isDown)
                        return FindFirstOccupiedSpot(6, 0, 7);
                    else if (isLeft)
                        return FindFirstOccupiedSpot(6, 7, 3, 4, 2, 5, 1);
                    else if (isRight)
                        return FindFirstOccupiedSpot(6, 5, 1, 4, 2, 3, 7);
                    break;

                case 7:
                    if (isUp)
                        return FindFirstOccupiedSpot(7, 0, 6);
                    else if (isDown)
                        return FindFirstOccupiedSpot(7, 6, 0);
                    else if (isLeft)
                        return FindFirstOccupiedSpot(7, 3, 2, 4, 1, 5, 0, 6);
                    else if (isRight)
                        return FindFirstOccupiedSpot(7, 0, 6, 1, 5, 2, 4, 3);
                    break;
            }
            return currentSpotIdx;
        }

        private int NavigateTheSameDirectionAgain(int _, int failedSpotIdx, InputEvent.NavigationDirection direction, MainWindowFocusManager.NavigationCallback callback)
            => callback(failedSpotIdx, direction);

        private int FindFirstOccupiedSpot(int fallback, params int[] indicesToCheck)
        {
            if (_moveInProgress)
                return indicesToCheck.First();

            foreach (var i in indicesToCheck)
            {
                if (PlayerSpots.First(x => x.SpotIndex == i).HasPlayerData != default)
                    return i;
            }
            return fallback;
        }
        #endregion
    }
}
