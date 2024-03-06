using System;
using System.Collections.Generic;
using System.Linq;
using PokerTracker3000.GameSession;

using InputEvent = PokerTracker3000.Input.InputManager.UserInputEvent;

namespace PokerTracker3000
{
    internal static class MainWindowFocusManager
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

        public static FocusArea CurrentFocusArea { get; private set; } = FocusArea.None;

        #region Private fields
        private static FocusArea s_lastFocusArea = FocusArea.None;
        private static int s_currentFocusedPlayerSpotIndex = -1;
        private static int s_lastFocusedPlayerSpot = -1;
        private static List<PlayerSpot>? s_playerSpots;
        private static Func<int, InputEvent.NavigationDirection, int>? s_spotNavigationCallback;
        private static Action<PlayerSpot, InputEvent.NavigationDirection>? s_playerOptionsNavigationCallback;
        private static Func<PlayerSpot, FocusArea>? s_playerOptionsSelectCallback;
        private static Action? s_editMenuLostFocusCallback;

        private static readonly Dictionary<InputEvent.ButtonEventType, Action> s_buttonPressedHandlers = new()
        {
            { InputEvent.ButtonEventType.Start, HandleStartButtonPressed },
            { InputEvent.ButtonEventType.Select, HandleSelectButtonPressed },
            { InputEvent.ButtonEventType.GoBack, HandleGoBackButtonPressed },
        };
        #endregion

        #region Public method

        #region Registration methods
        public static void RegisterPlayerSpots(List<PlayerSpot> playerSpots)
        {
            s_playerSpots = playerSpots;
        }

        public static void RegisterSpotNavigationCallback(Func<int, InputEvent.NavigationDirection, int> callback)
        {
            s_spotNavigationCallback = callback;
        }

        public static void RegisterPlayerOptionsCallback(Action<PlayerSpot, InputEvent.NavigationDirection> callback)
        {
            s_playerOptionsNavigationCallback = callback;
        }

        public static void RegisterPlayerInfoBoxSelectCallback(Func<PlayerSpot, FocusArea> callback)
        {
            s_playerOptionsSelectCallback = callback;
        }

        public static void RegisterEditMenuLostFocusCallback(Action callback)
        {
            s_editMenuLostFocusCallback = callback;
        }
        #endregion

        public static void HandleButtonPressedEvent(InputEvent.ButtonEventType button)
        {
            if (s_buttonPressedHandlers.TryGetValue(button, out var handler))
                handler.Invoke();
        }

        public static void HandleNavigationEvent(InputEvent.NavigationDirection direction)
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
                    if (s_playerOptionsNavigationCallback != default && TryGetMatchingSpot(x => x.IsSelected, out var spot))
                        s_playerOptionsNavigationCallback(spot!, direction);
                    break;
            }
        }
        #endregion

        #region Private methods
        private static void HandleStartButtonPressed()
        {
            if (CurrentFocusArea == FocusArea.LeftSideMenu)
                RestoreFromSideMenu();
            else
                SetNewFocusArea(FocusArea.LeftSideMenu);
        }

        private static void HandleGoBackButtonPressed()
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

        private static void HandleSelectButtonPressed()
        {
            switch (CurrentFocusArea)
            {
                case FocusArea.Players:
                    SetNewFocusArea(FocusArea.PlayerInfo);
                    break;

                case FocusArea.PlayerInfo:
                    if (s_playerOptionsSelectCallback != default && TryGetMatchingSpot(x => x.IsSelected, out var spot))
                    {
                        var newFocusArea = s_playerOptionsSelectCallback.Invoke(spot!);
                        SetNewFocusArea(newFocusArea);
                    }
                    break;

                case FocusArea.EditNameBox:
                    SetNewFocusArea(FocusArea.PlayerInfo);
                    break;
            }
        }

        private static void RestoreFromSideMenu()
        {
            SetNewFocusArea(s_lastFocusArea);

            // Note: If we're restoring, we already know what area to move the focus to.
            //       Hence, we don't need to ask the the view where to go. However, we
            //       have no other way of letting the view know that it has regained focus,
            //       so we have to call the callback anyway.
            if (CurrentFocusArea == FocusArea.EditNameBox && s_playerOptionsSelectCallback != default)
            {
                if (TryGetMatchingSpot(x => x.SpotIndex == s_currentFocusedPlayerSpotIndex, out var activeSpot))
                    _ = s_playerOptionsSelectCallback.Invoke(activeSpot!);
            }
        }

        private static void FocusPlayerArea()
        {
            if (s_playerSpots == default)
                return;

            s_lastFocusedPlayerSpot = s_lastFocusedPlayerSpot == -1 ? 0 : s_lastFocusedPlayerSpot;
            SetNewFocusArea(FocusArea.Players);
        }

        private static void SetNewFocusArea(FocusArea newFocusArea)
        {
            ClearFocusFromCurrentlyFocusedArea();

            PlayerSpot? activeSpot = default;
            if (newFocusArea == FocusArea.Players || newFocusArea == FocusArea.PlayerInfo || newFocusArea == FocusArea.EditNameBox)
            {
                if (!TryGetMatchingSpot(x => x.SpotIndex == s_lastFocusedPlayerSpot, out activeSpot))
                    return;
            }
            s_lastFocusArea = CurrentFocusArea;
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
                        s_currentFocusedPlayerSpotIndex = activeSpot.SpotIndex;
                        s_lastFocusedPlayerSpot = -1;
                    }
                    break;

                case FocusArea.LeftSideMenu:
                    break;
            }
        }

        private static void HandlePlayersAreaNavigationEvent(InputEvent.NavigationDirection direction)
        {
            if (s_spotNavigationCallback == default)
                return;

            var newFocusIndex = s_spotNavigationCallback.Invoke(s_currentFocusedPlayerSpotIndex, direction);
            if (TryGetMatchingSpot(x => x.SpotIndex == newFocusIndex, out var spotToFocus))
            {
                if (TryGetMatchingSpot(x => x.SpotIndex == s_currentFocusedPlayerSpotIndex, out var oldFocusedSpot))
                    oldFocusedSpot!.IsHighlighted = false;
                spotToFocus!.IsHighlighted = true;
                s_currentFocusedPlayerSpotIndex = newFocusIndex;
            }
        }

        private static void ClearFocusFromCurrentlyFocusedArea()
        {
            switch (CurrentFocusArea)
            {
                case FocusArea.Players:
                case FocusArea.PlayerInfo:
                case FocusArea.EditNameBox:
                    if (!TryGetMatchingSpot(x => CurrentFocusArea == FocusArea.Players ? x.IsHighlighted : x.IsSelected, out var focusedSpot))
                        return;

                    s_lastFocusedPlayerSpot = focusedSpot!.SpotIndex;
                    s_currentFocusedPlayerSpotIndex = -1;
                    if (CurrentFocusArea == FocusArea.PlayerInfo)
                        focusedSpot.IsSelected = false;
                    focusedSpot.IsHighlighted = false;

                    if (CurrentFocusArea == FocusArea.EditNameBox && s_editMenuLostFocusCallback != default)
                        s_editMenuLostFocusCallback.Invoke();
                    break;
            }
        }

        private static bool TryGetMatchingSpot(Predicate<PlayerSpot> predicate, out PlayerSpot? spot)
        {
            spot = default;
            if (s_playerSpots == default)
                return false;

            spot = s_playerSpots.FirstOrDefault(x => predicate(x));
            return spot != default;
        }
        #endregion
    }
}
