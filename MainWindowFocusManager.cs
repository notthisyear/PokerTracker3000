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
        }

        public static FocusArea CurrentFocusArea { get; private set; } = FocusArea.None;

        #region Private fields
        private static FocusArea s_lastFocusArea = FocusArea.None;
        private static int s_currentFocusedPlayerSpotIndex = -1;
        private static int s_lastFocusedPlayerSpot = -1;
        private static List<PlayerSpot>? s_playerSpots;
        private static Func<int, InputEvent.NavigationDirection, int>? s_spotNavigationCallback;
        private static Action<PlayerSpot, InputEvent.NavigationDirection>? s_playerOptionsNavigationCallback;
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
        #endregion

        public static void HandleButtonPressedEvent(InputEvent.ButtonEventType button)
        {
            switch (button)
            {
                case InputEvent.ButtonEventType.Start:
                    if (CurrentFocusArea == FocusArea.LeftSideMenu)
                        RestoreFocusArea();
                    else
                        SetNewFocusArea(FocusArea.LeftSideMenu);
                    break;

                case InputEvent.ButtonEventType.Select:
                    switch (CurrentFocusArea)
                    {
                        case FocusArea.Players:
                            SetNewFocusArea(FocusArea.PlayerInfo);
                            break;
                    }
                    break;

                case InputEvent.ButtonEventType.GoBack:
                    switch (CurrentFocusArea)
                    {
                        case FocusArea.Players:
                            SetNewFocusArea(FocusArea.None);
                            break;

                        case FocusArea.PlayerInfo:
                            SetNewFocusArea(FocusArea.Players);
                            break;

                    }
                    break;
            }
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
        private static void RestoreFocusArea()
            => SetNewFocusArea(s_lastFocusArea);

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
            if (newFocusArea == FocusArea.Players || newFocusArea == FocusArea.PlayerInfo)
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
                    if (activeSpot != default)
                    {
                        activeSpot.IsHighlighted = true;
                        if (newFocusArea == FocusArea.PlayerInfo)
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
                    if (!TryGetMatchingSpot(x => CurrentFocusArea == FocusArea.Players ? x.IsHighlighted : x.IsSelected, out var focusedSpot))
                        return;

                    s_lastFocusedPlayerSpot = focusedSpot!.SpotIndex;
                    s_currentFocusedPlayerSpotIndex = -1;
                    if (CurrentFocusArea == FocusArea.PlayerInfo)
                        focusedSpot.IsSelected = false;
                    focusedSpot.IsHighlighted = false;
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
