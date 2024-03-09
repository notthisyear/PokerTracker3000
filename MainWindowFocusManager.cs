﻿using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.GameSession;

using InputEvent = PokerTracker3000.Input.InputManager.UserInputEvent;

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
            ConfirmationDialog
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
        public delegate int NavigationCallback(int spotIdx, InputEvent.NavigationDirection direction);
        public delegate void PlayerOptionNavigationCallback(PlayerSpot spot, InputEvent.NavigationDirection direction);
        public delegate FocusArea PlayerOptionSelectCallback(PlayerSpot spot);
        public delegate void EditMenuLostFocusCallback();
        #endregion

        #region Private fields
        private FocusArea _lastFocusArea = FocusArea.None;
        private int _currentFocusedPlayerSpotIndex = -1;
        private int _lastFocusedPlayerSpotIndex = -1;
        private List<PlayerSpot>? _playerSpots;
        private NavigationCallback? _spotNavigationCallback;
        private PlayerOptionNavigationCallback? _playerOptionsNavigationCallback;
        private PlayerOptionSelectCallback? _playerOptionsSelectCallback;
        private EditMenuLostFocusCallback? _editMenuLostFocusCallback;

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
        #region Public method

        #region Registration methods
        public void RegisterPlayerSpots(List<PlayerSpot> playerSpots)
        {
            _playerSpots = playerSpots;
        }

        public void RegisterSpotNavigationCallback(NavigationCallback callback)
        {
            _spotNavigationCallback = callback;
        }

        public void RegisterPlayerOptionsCallback(PlayerOptionNavigationCallback callback)
        {
            _playerOptionsNavigationCallback = callback;
        }

        public void RegisterPlayerInfoBoxSelectCallback(PlayerOptionSelectCallback callback)
        {
            _playerOptionsSelectCallback = callback;
        }

        public void RegisterEditMenuLostFocusCallback(EditMenuLostFocusCallback callback)
        {
            _editMenuLostFocusCallback = callback;
        }
        #endregion

        public void HandleButtonPressedEvent(InputEvent.ButtonEventType button)
        {
            if (_buttonPressedHandlers.TryGetValue(button, out var handler))
                handler.Invoke();
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
                    // TODO
                    break;

                case FocusArea.Players:
                    HandlePlayersAreaNavigationEvent(direction);
                    break;

                case FocusArea.PlayerInfo:
                    if (_playerOptionsNavigationCallback != default && TryGetMatchingSpot(x => x.IsSelected, out var spot))
                        _playerOptionsNavigationCallback(spot!, direction);
                    break;
            }
        }
        #endregion

        #region Private methods
        private void HandleStartButtonPressed()
        {
            if (CurrentFocusArea == FocusArea.LeftSideMenu)
                RestoreFromSideMenu();
            else
                SetNewFocusArea(FocusArea.LeftSideMenu);
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
                    SetNewFocusArea(FocusArea.PlayerInfo);
                    break;
            }
        }

        private void HandleSelectButtonPressed()
        {
            switch (CurrentFocusArea)
            {
                case FocusArea.Players:
                    SetNewFocusArea(FocusArea.PlayerInfo);
                    break;

                case FocusArea.PlayerInfo:
                    if (_playerOptionsSelectCallback != default && TryGetMatchingSpot(x => x.IsSelected, out var spot))
                    {
                        var newFocusArea = _playerOptionsSelectCallback.Invoke(spot!);
                        SetNewFocusArea(newFocusArea);
                    }
                    break;

                case FocusArea.EditNameBox:
                    SetNewFocusArea(FocusArea.PlayerInfo);
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
            if (_lastFocusArea == FocusArea.EditNameBox && _playerOptionsSelectCallback != default)
            {
                if (TryGetMatchingSpot(x => x.SpotIndex == _lastFocusedPlayerSpotIndex, out var lastActiveSpot))
                    _ = _playerOptionsSelectCallback.Invoke(lastActiveSpot!);
            }
            SetNewFocusArea(_lastFocusArea);
        }

        private void FocusPlayerArea()
        {
            _lastFocusedPlayerSpotIndex = _lastFocusedPlayerSpotIndex == -1 ? 0 : _lastFocusedPlayerSpotIndex;
            SetNewFocusArea(FocusArea.Players);
        }

        private void SetNewFocusArea(FocusArea newFocusArea)
        {
            ClearFocusFromCurrentlyFocusedArea();

            PlayerSpot? activeSpot = default;
            if (newFocusArea == FocusArea.Players || newFocusArea == FocusArea.PlayerInfo || newFocusArea == FocusArea.EditNameBox)
            {
                if (!TryGetMatchingSpot(x => x.SpotIndex == _lastFocusedPlayerSpotIndex, out activeSpot))
                    return;
            }
            _lastFocusArea = CurrentFocusArea;
            CurrentFocusArea = newFocusArea;

            switch (newFocusArea)
            {
                case FocusArea.Players:
                case FocusArea.PlayerInfo:
                case FocusArea.EditNameBox:
                    if (activeSpot != default)
                    {
                        activeSpot.IsHighlighted = true;
                        if (newFocusArea == FocusArea.PlayerInfo || newFocusArea == FocusArea.EditNameBox)
                            activeSpot.IsSelected = true;
                        _currentFocusedPlayerSpotIndex = activeSpot.SpotIndex;
                        _lastFocusedPlayerSpotIndex = -1;

                        if (newFocusArea == FocusArea.PlayerInfo && _playerSpots != default)
                            activeSpot.CanBeRemoved = _playerSpots.Where(x => x.HasPlayerData).Count() > 1;
                    }
                    break;
                case FocusArea.LeftSideMenu:
                    break;
            }
        }

        private void HandlePlayersAreaNavigationEvent(InputEvent.NavigationDirection direction)
        {
            if (_spotNavigationCallback == default)
                return;

            var newFocusIndex = _spotNavigationCallback.Invoke(_currentFocusedPlayerSpotIndex, direction);
            if (!TryGetMatchingSpot(x => x.SpotIndex == newFocusIndex, out var spotToFocus) || !spotToFocus!.HasPlayerData)
                return;

            if (TryGetMatchingSpot(x => x.SpotIndex == _currentFocusedPlayerSpotIndex, out var oldFocusedSpot))
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
                    if (!TryGetMatchingSpot(x => CurrentFocusArea == FocusArea.Players ? x.IsHighlighted : x.IsSelected, out var focusedSpot))
                        return;

                    _lastFocusedPlayerSpotIndex = focusedSpot!.SpotIndex;
                    _currentFocusedPlayerSpotIndex = -1;
                    if (CurrentFocusArea == FocusArea.PlayerInfo)
                        focusedSpot.IsSelected = false;
                    focusedSpot.IsHighlighted = false;

                    if (CurrentFocusArea == FocusArea.EditNameBox && _editMenuLostFocusCallback != default)
                        _editMenuLostFocusCallback.Invoke();
                    break;
            }
        }

        private bool TryGetMatchingSpot(Predicate<PlayerSpot> predicate, out PlayerSpot? spot)
        {
            spot = default;
            if (_playerSpots == default)
                return false;

            spot = _playerSpots.FirstOrDefault(x => predicate(x));
            return spot != default;
        }
        #endregion
    }
}
