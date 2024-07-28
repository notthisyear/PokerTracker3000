using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.GameSession;

using InputEvent = PokerTracker3000.Input.UserInputEvent;

namespace PokerTracker3000
{
    public class MainWindowFocusManager : ObservableObject
    {
        public enum FocusArea
        {
            None,
            LeftSideMenu,
            Players,
            PlayerInfo,
            EditNameBox,
            AddOnOrBuyInBox,
            ConfirmationDialog,
            MovementInProgress,
            SideMenuEditOption,
            SpotifyInformationBox
        }

        #region Public properties

        #region Backing fields
        public FocusArea _currentFocusArea = FocusArea.None;
        #endregion

        public FocusArea CurrentFocusArea
        {
            get => _currentFocusArea;
            private set => SetProperty(ref _currentFocusArea, value);
        }
        #endregion

        #region Callback delegates
        public delegate int SpotNavigationCallback(int currentIndex, InputEvent.NavigationDirection direction);
        public delegate void NavigationCallback(InputEvent.NavigationDirection direction);
        public delegate void SideMenuVisibilityChangedCallback(bool isVisible);
        public delegate bool SideMenuButtonCallback(InputEvent.ButtonEventType eventType);
        public delegate void SpotSelectedCallback(PlayerSpot activeSpot, int spotIndex);
        public delegate void PlayerOptionNavigationCallback(PlayerSpot spot, InputEvent.NavigationDirection direction);
        public delegate FocusArea PlayerOptionSelectCallback(PlayerSpot spot);
        public delegate void EditMenuLostFocusCallback();
        public delegate bool BuyInOrAddOnOptionSelectedCallback();
        public delegate void PlayerMovementDoneCallback(PlayerSpot spot);
        #endregion

        #region Private fields
        private FocusArea _lastFocusArea = FocusArea.None;
        private int _currentFocusedPlayerSpotIndex = -1;
        private int _lastFocusedPlayerSpotIndex = -1;

        private List<PlayerSpot>? _playerSpots;
        private SpotNavigationCallback? _spotNavigationCallback;
        private NavigationCallback? _sideMenuNavigationCallback;
        private SideMenuButtonCallback? _sideMenuButtonCallback;
        private SideMenuVisibilityChangedCallback? _sideMenuVisibilityChangedCallback;
        private SpotSelectedCallback? _spotSelectedCallback;
        private PlayerOptionNavigationCallback? _playerOptionsNavigationCallback;
        private PlayerOptionSelectCallback? _playerOptionsSelectCallback;
        private NavigationCallback? _buyInOrAddOnNavigationCallback;
        private EditMenuLostFocusCallback? _editMenuLostFocusCallback;
        private BuyInOrAddOnOptionSelectedCallback? _buyInOrAddOnOptionSelected;
        private PlayerMovementDoneCallback? _playerMovementDoneCallback;
        private NavigationCallback? _sideMenuEditOptionNavigationCallback;
        private SideMenuButtonCallback? _sideMenuEditOptionActionCallback;

        private readonly Dictionary<InputEvent.ButtonEventType, Action> _buttonPressedHandlers;
        #endregion

        public MainWindowFocusManager()
        {
            _buttonPressedHandlers = new()
            {
                { InputEvent.ButtonEventType.Start, HandleStartButtonPressed },
                { InputEvent.ButtonEventType.Select, HandleSelectButtonPressed },
                { InputEvent.ButtonEventType.GoBack, HandleGoBackButtonPressed },
            };
        }

        #region Public methods

        #region Registration methods
        public void RegisterPlayerSpots(List<PlayerSpot> playerSpots)
        {
            _playerSpots = playerSpots;
        }

        public void RegisterSideVisibilityChangedCallback(SideMenuVisibilityChangedCallback callback)
        {
            _sideMenuVisibilityChangedCallback = callback;
        }

        public void RegisterSpotNavigationCallback(SpotNavigationCallback callback)
        {
            _spotNavigationCallback = callback;
        }

        public void RegisterSideMenuNavigationCallback(NavigationCallback callback)
        {
            _sideMenuNavigationCallback = callback;
        }

        public void RegisterSideMenuButtonCallback(SideMenuButtonCallback callback)
        {
            _sideMenuButtonCallback = callback;
        }

        public void RegisterSpotSelectedCallback(SpotSelectedCallback callback)
        {
            _spotSelectedCallback = callback;
        }

        public void RegisterPlayerOptionsCallback(PlayerOptionNavigationCallback callback)
        {
            _playerOptionsNavigationCallback = callback;
        }

        public void RegisterPlayerOptionSelectCallback(PlayerOptionSelectCallback callback)
        {
            _playerOptionsSelectCallback = callback;
        }

        public void RegisterBuyInOrAddOnBoxNavigationCallback(NavigationCallback callback)
        {
            _buyInOrAddOnNavigationCallback = callback;
        }

        public void RegisterEditMenuLostFocusCallback(EditMenuLostFocusCallback callback)
        {
            _editMenuLostFocusCallback = callback;
        }

        public void RegisterBuyInOrAddOnOptionSelectedCallback(BuyInOrAddOnOptionSelectedCallback callback)
        {
            _buyInOrAddOnOptionSelected = callback;
        }

        public void RegisterMovementDoneCallback(PlayerMovementDoneCallback callback)
        {
            _playerMovementDoneCallback = callback;
        }

        public void RegisterSideMenuEditOptionNavigationCallback(NavigationCallback callback)
        {
            _sideMenuEditOptionNavigationCallback = callback;
        }

        public void RegisterSideMenuEditOptionActionCallback(SideMenuButtonCallback callback)
        {
            _sideMenuEditOptionActionCallback = callback;
        }
        #endregion

        public void HandleButtonPressedEvent(InputEvent.ButtonEventType button)
        {
            if (_buttonPressedHandlers.TryGetValue(button, out var handler))
                handler.Invoke();

            Debug.WriteLine($"_lastFocusArea: {_lastFocusArea}");
            Debug.WriteLine($"_currentFocusedPlayerSpotIndex: {_currentFocusedPlayerSpotIndex}");
            Debug.WriteLine($"_lastFocusedPlayerSpot: {_lastFocusedPlayerSpotIndex}");
            Debug.WriteLine($"CurrentFocusArea: {CurrentFocusArea}\n");
        }

        public void HandleNavigationEvent(InputEvent.NavigationDirection direction)
        {
            if (CurrentFocusArea == FocusArea.None)
            {
                FocusPlayerArea();
                return;
            }

            switch (CurrentFocusArea)
            {
                case FocusArea.LeftSideMenu:
                    _sideMenuNavigationCallback?.Invoke(direction);
                    break;

                case FocusArea.Players:
                case FocusArea.MovementInProgress:
                    HandlePlayersAreaNavigationEvent(direction);
                    break;

                case FocusArea.PlayerInfo:
                    if (_playerOptionsNavigationCallback != default && TryGetMatching(_playerSpots, x => x.IsSelected, out var spot))
                        _playerOptionsNavigationCallback(spot!, direction);
                    break;

                case FocusArea.AddOnOrBuyInBox:
                    _buyInOrAddOnNavigationCallback?.Invoke(direction);
                    break;

                case FocusArea.SideMenuEditOption:
                    _sideMenuEditOptionNavigationCallback?.Invoke(direction);
                    break;

            }
        }

        public void SideMenuEditOptionSelected()
        {
            if (CurrentFocusArea == FocusArea.LeftSideMenu)
                CurrentFocusArea = FocusArea.SideMenuEditOption;
        }
        #endregion

        #region Private methods
        private void HandleStartButtonPressed()
        {
            if (CurrentFocusArea == FocusArea.LeftSideMenu)
            {
                RestoreFromSideMenu();
            }
            else
            {
                var saveCurrentFocusAsLastFocusArea = true;
                if (CurrentFocusArea == FocusArea.SideMenuEditOption)
                {
                    _ = _sideMenuEditOptionActionCallback?.Invoke(InputEvent.ButtonEventType.Start);
                    // This ensure that we don't overwrite what the main focus was
                    // before we entered the game settings options
                    saveCurrentFocusAsLastFocusArea = false;
                }
                SetNewFocusArea(FocusArea.LeftSideMenu, saveCurrentFocusAsLastFocusArea);
            }
            _sideMenuVisibilityChangedCallback?.Invoke(CurrentFocusArea == FocusArea.LeftSideMenu);
        }

        private void HandleGoBackButtonPressed()
        {
            switch (CurrentFocusArea)
            {
                case FocusArea.Players:
                    SetNewFocusArea(FocusArea.None);
                    break;

                case FocusArea.PlayerInfo:
                    SetNewFocusArea(FocusArea.Players);
                    break;

                case FocusArea.EditNameBox:
                case FocusArea.AddOnOrBuyInBox:
                    SetNewFocusArea(FocusArea.PlayerInfo);
                    break;

                case FocusArea.MovementInProgress:
                    {
                        if (_playerMovementDoneCallback != default && TryGetMatching(_playerSpots, x => x.IsBeingMoved, out var spot))
                            _playerMovementDoneCallback.Invoke(spot!);
                    }
                    SetNewFocusArea(FocusArea.Players);
                    break;

                case FocusArea.LeftSideMenu:
                    if (_sideMenuButtonCallback != default)
                    {
                        if (_sideMenuButtonCallback.Invoke(InputEvent.ButtonEventType.GoBack))
                            RestoreFromSideMenu();
                    }
                    break;

                case FocusArea.SideMenuEditOption:
                    if (_sideMenuEditOptionActionCallback != default)
                    {
                        if (_sideMenuEditOptionActionCallback.Invoke(InputEvent.ButtonEventType.GoBack))
                            SetNewFocusArea(FocusArea.LeftSideMenu, false);
                    }
                    break;
            }
        }

        private void HandleSelectButtonPressed()
        {
            switch (CurrentFocusArea)
            {
                case FocusArea.Players:
                    {
                        SetNewFocusArea(FocusArea.PlayerInfo);
                        if (_spotSelectedCallback != default && TryGetMatching(_playerSpots, x => x.IsSelected, out var spot))
                            _spotSelectedCallback.Invoke(spot!, _currentFocusedPlayerSpotIndex);
                    }
                    break;

                case FocusArea.PlayerInfo:
                    {
                        if (_playerOptionsSelectCallback != default && TryGetMatching(_playerSpots, x => x.IsSelected, out var spot))
                        {
                            var newFocusArea = _playerOptionsSelectCallback.Invoke(spot!);
                            SetNewFocusArea(newFocusArea);
                        }
                    }
                    break;

                case FocusArea.EditNameBox:
                    SetNewFocusArea(FocusArea.PlayerInfo);
                    break;

                case FocusArea.AddOnOrBuyInBox:
                    if (_buyInOrAddOnOptionSelected != default && _buyInOrAddOnOptionSelected.Invoke())
                        SetNewFocusArea(FocusArea.PlayerInfo);
                    break;

                case FocusArea.MovementInProgress:
                    {
                        if (_playerMovementDoneCallback != default && TryGetMatching(_playerSpots, x => x.IsBeingMoved, out var spot))
                            _playerMovementDoneCallback.Invoke(spot!);
                    }
                    SetNewFocusArea(FocusArea.Players);
                    break;

                case FocusArea.LeftSideMenu:
                    _ = _sideMenuButtonCallback?.Invoke(InputEvent.ButtonEventType.Select);
                    break;

                case FocusArea.SideMenuEditOption:
                    _ = _sideMenuEditOptionActionCallback?.Invoke(InputEvent.ButtonEventType.Select);
                    break;
            }
        }

        private void RestoreFromSideMenu()
        {
            // Note: If we're restoring, we already know what area to move the focus to.
            //       Hence, we don't need to ask where to go. However, in the case where
            //       we're going back to a edit box focus, we need to let the session
            //       manager know that a focus change is upcoming, so we call the
            //       callback anyway.
            if ((_lastFocusArea == FocusArea.EditNameBox || _lastFocusArea == FocusArea.AddOnOrBuyInBox) &&
                _playerOptionsSelectCallback != default)
            {
                if (TryGetMatching(_playerSpots, x => x.SpotIndex == _lastFocusedPlayerSpotIndex, out var lastActiveSpot))
                    _ = _playerOptionsSelectCallback.Invoke(lastActiveSpot!);
            }

            SetNewFocusArea(_lastFocusArea);
        }

        private void FocusPlayerArea()
        {
            _lastFocusedPlayerSpotIndex = _lastFocusedPlayerSpotIndex == -1 ? 0 : _lastFocusedPlayerSpotIndex;
            SetNewFocusArea(FocusArea.Players);
        }

        private void SetNewFocusArea(FocusArea newFocusArea, bool saveCurrentFocusArea = true)
        {
            ClearFocusFromCurrentlyFocusedArea();

            PlayerSpot? activeSpot = default;
            if (newFocusArea == FocusArea.Players ||
                newFocusArea == FocusArea.PlayerInfo ||
                newFocusArea == FocusArea.EditNameBox ||
                newFocusArea == FocusArea.AddOnOrBuyInBox ||
                newFocusArea == FocusArea.MovementInProgress)
            {
                if (!TryGetMatching(_playerSpots, x => x.SpotIndex == _lastFocusedPlayerSpotIndex, out activeSpot))
                    return;
            }

            if (saveCurrentFocusArea)
                _lastFocusArea = CurrentFocusArea;
            CurrentFocusArea = newFocusArea;

            switch (newFocusArea)
            {
                case FocusArea.Players:
                case FocusArea.PlayerInfo:
                case FocusArea.EditNameBox:
                case FocusArea.MovementInProgress:
                case FocusArea.AddOnOrBuyInBox:
                    if (activeSpot != default)
                    {
                        activeSpot.IsHighlighted = true;
                        if (newFocusArea == FocusArea.PlayerInfo ||
                            newFocusArea == FocusArea.EditNameBox ||
                            newFocusArea == FocusArea.AddOnOrBuyInBox)
                        {
                            activeSpot.IsSelected = true;
                        }
                        _currentFocusedPlayerSpotIndex = activeSpot.SpotIndex;
                        _lastFocusedPlayerSpotIndex = -1;
                    }
                    break;

                default:
                    break;
            }
        }

        private void HandlePlayersAreaNavigationEvent(InputEvent.NavigationDirection direction)
        {
            if (_spotNavigationCallback == default)
                return;

            var newFocusIndex = _spotNavigationCallback.Invoke(_currentFocusedPlayerSpotIndex, direction);
            if (!TryGetMatching(_playerSpots, x => x.SpotIndex == newFocusIndex, out var spotToFocus) || !spotToFocus!.HasPlayerData)
                return;

            if (TryGetMatching(_playerSpots, x => x.SpotIndex == _currentFocusedPlayerSpotIndex, out var oldFocusedSpot))
                oldFocusedSpot!.IsHighlighted = false;

            spotToFocus!.IsHighlighted = true;
            _currentFocusedPlayerSpotIndex = newFocusIndex;
        }

        private void ClearFocusFromCurrentlyFocusedArea()
        {
            switch (CurrentFocusArea)
            {
                case FocusArea.Players:
                case FocusArea.PlayerInfo:
                case FocusArea.EditNameBox:
                case FocusArea.AddOnOrBuyInBox:
                case FocusArea.MovementInProgress:
                    Predicate<PlayerSpot> propertyToCheck =
                        (CurrentFocusArea == FocusArea.Players || CurrentFocusArea == FocusArea.MovementInProgress) ?
                        (x => x.IsHighlighted) : (x => x.IsSelected);

                    if (!TryGetMatching(_playerSpots, propertyToCheck, out var focusedSpot))
                        return;

                    _lastFocusedPlayerSpotIndex = focusedSpot!.SpotIndex;
                    _currentFocusedPlayerSpotIndex = -1;
                    if (CurrentFocusArea == FocusArea.PlayerInfo)
                        focusedSpot.IsSelected = false;
                    focusedSpot.IsHighlighted = false;

                    var inEditArea = CurrentFocusArea == FocusArea.EditNameBox || CurrentFocusArea == FocusArea.AddOnOrBuyInBox;
                    if (inEditArea)
                        _editMenuLostFocusCallback?.Invoke();
                    break;
            }
        }

        private static bool TryGetMatching<T>(List<T>? list, Predicate<T> predicate, out T? result) where T : class
        {
            result = default;
            if (list == default)
                return false;

            result = list.FirstOrDefault(x => predicate(x));
            return result != default;
        }
        #endregion
    }
}
