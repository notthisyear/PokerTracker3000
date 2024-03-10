using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static PokerTracker3000.MainWindowFocusManager;

using InputEvent = PokerTracker3000.Input.InputManager.UserInputEvent;

namespace PokerTracker3000.GameSession
{
    internal class NavigationManager
    {
        #region Private fields
        private readonly ReadOnlyCollection<PlayerSpot> _spots;
        private readonly Dictionary<TableLayout, NavigationCallback> _layoutNavigation;
        private bool _moveInProgress;
        #endregion

        public NavigationManager(ReadOnlyCollection<PlayerSpot> spots)
        {
            _spots = spots;
            _layoutNavigation = new()
            {
                { TableLayout.TwoPlayers, NavigateTwoPlayerLayout },
                { TableLayout.FourPlayers, NavigateFourPlayerLayout },
                { TableLayout.SixPlayers, NavigateSixPlayerLayout },
                { TableLayout.EightPlayers, NavigateEightPlayersLayout }
            };
        }

        public int Navigate(TableLayout layout, int currentSpotIdx, InputEvent.NavigationDirection direction, bool moveInProgress)
        {
            _moveInProgress = moveInProgress;
            if (_layoutNavigation.TryGetValue(layout, out var navigationCallback))
                return navigationCallback(currentSpotIdx, direction);
            return currentSpotIdx;
        }

        #region Private methods
        private int NavigateTwoPlayerLayout(int currentSpotIdx, InputEvent.NavigationDirection _)
           /* Layout:
            *
            *   0
            *
            *   1
            */
           => FindFirstOccupiedSpot(currentSpotIdx, currentSpotIdx + 1 % 2);

        private int NavigateFourPlayerLayout(int currentSpotIdx, InputEvent.NavigationDirection direction)
        {
            /* Layout:
             *
             *   0  1
             *
             *   2  3
             *   
             * 0:  -> 2, 3 [up, down]     1:  -> 3, 2 [up, down]
             *     -> 1, 3 [left, right]      -> 0, 2 [left, right]
             *
             * 2:  -> 0, 1 [up, down]     3:  -> 0, 0 [up, down]
             *     -> 3, 1 [left, right]      -> 2, 0 [left, right]
            */
            if (direction == InputEvent.NavigationDirection.Up || direction == InputEvent.NavigationDirection.Down)
                return FindFirstOccupiedSpot(currentSpotIdx, (currentSpotIdx + 2) % 4, 3 - currentSpotIdx);
            return FindFirstOccupiedSpot(currentSpotIdx, currentSpotIdx + ((currentSpotIdx % 2 == 0) ? 1 : -1), 3 - currentSpotIdx);
        }

        private int NavigateSixPlayerLayout(int currentSpotIdx, InputEvent.NavigationDirection direction)
        {
            /* Layout:
             *
             *   0  1  2
             *
             *   3  4  5
             */
            return currentSpotIdx switch
            {
                0 => direction switch
                {
                    InputEvent.NavigationDirection.Up or
                    InputEvent.NavigationDirection.Down => FindFirstOccupiedSpot(0, 3, 4, 5),
                    InputEvent.NavigationDirection.Left => FindFirstOccupiedSpot(0, 2, 5, 1, 4),
                    InputEvent.NavigationDirection.Right => FindFirstOccupiedSpot(0, 1, 4, 2, 5),
                    _ => currentSpotIdx
                },
                1 => direction switch
                {
                    InputEvent.NavigationDirection.Up or
                    InputEvent.NavigationDirection.Down => FindFirstOccupiedSpot(1, 4, 3, 5),
                    InputEvent.NavigationDirection.Left => FindFirstOccupiedSpot(1, 0, 3, 5, 2),
                    InputEvent.NavigationDirection.Right => FindFirstOccupiedSpot(1, 2, 5, 0, 3),
                    _ => currentSpotIdx
                },
                2 => direction switch
                {
                    InputEvent.NavigationDirection.Up or
                    InputEvent.NavigationDirection.Down => FindFirstOccupiedSpot(2, 5, 4, 3),
                    InputEvent.NavigationDirection.Left => FindFirstOccupiedSpot(2, 1, 4, 0, 3),
                    InputEvent.NavigationDirection.Right => FindFirstOccupiedSpot(2, 0, 3, 1, 4),
                    _ => currentSpotIdx
                },
                3 => direction switch
                {
                    InputEvent.NavigationDirection.Up or
                    InputEvent.NavigationDirection.Down => FindFirstOccupiedSpot(3, 0, 1, 2),
                    InputEvent.NavigationDirection.Left => FindFirstOccupiedSpot(3, 5, 2, 4, 1),
                    InputEvent.NavigationDirection.Right => FindFirstOccupiedSpot(3, 4, 1, 2, 5),
                    _ => currentSpotIdx
                },
                4 => direction switch
                {
                    InputEvent.NavigationDirection.Up or
                    InputEvent.NavigationDirection.Down => FindFirstOccupiedSpot(4, 1, 0, 2),
                    InputEvent.NavigationDirection.Left => FindFirstOccupiedSpot(4, 3, 0, 5, 2),
                    InputEvent.NavigationDirection.Right => FindFirstOccupiedSpot(4, 5, 2, 3, 0),
                    _ => currentSpotIdx
                },
                5 => direction switch
                {
                    InputEvent.NavigationDirection.Up or
                    InputEvent.NavigationDirection.Down => FindFirstOccupiedSpot(5, 2, 1, 0),
                    InputEvent.NavigationDirection.Left => FindFirstOccupiedSpot(5, 4, 1, 3, 0),
                    InputEvent.NavigationDirection.Right => FindFirstOccupiedSpot(5, 3, 0, 4, 1),
                    _ => currentSpotIdx
                },
                _ => currentSpotIdx
            };
        }

        private int NavigateEightPlayersLayout(int currentSpotIdx, InputEvent.NavigationDirection direction)
        {
            /* Layout:
             *
             *   0  1  2
             * 7         3
             *   6  5  4
             */
            return currentSpotIdx switch
            {
                0 => direction switch
                {
                    InputEvent.NavigationDirection.Up or
                    InputEvent.NavigationDirection.Down => FindFirstOccupiedSpot(0, 6, 7, 5, 4, 3),
                    InputEvent.NavigationDirection.Left => FindFirstOccupiedSpot(0, 7, 3, 2, 4, 1, 5),
                    InputEvent.NavigationDirection.Right => FindFirstOccupiedSpot(0, 1, 5, 2, 4, 3, 7),
                    _ => currentSpotIdx
                },
                1 => direction switch
                {
                    InputEvent.NavigationDirection.Up or
                    InputEvent.NavigationDirection.Down => FindFirstOccupiedSpot(1, 5, 6, 4, 7, 3),
                    InputEvent.NavigationDirection.Left => FindFirstOccupiedSpot(1, 0, 6, 7, 3, 2, 4),
                    InputEvent.NavigationDirection.Right => FindFirstOccupiedSpot(1, 2, 4, 3, 7, 0, 6),
                    _ => currentSpotIdx
                },
                2 => direction switch
                {
                    InputEvent.NavigationDirection.Up or
                    InputEvent.NavigationDirection.Down => FindFirstOccupiedSpot(2, 4, 3, 5, 6, 7),
                    InputEvent.NavigationDirection.Left => FindFirstOccupiedSpot(2, 1, 5, 0, 6, 7, 3),
                    InputEvent.NavigationDirection.Right => FindFirstOccupiedSpot(2, 3, 7, 0, 6, 1, 5),
                    _ => currentSpotIdx
                },
                3 => direction switch
                {
                    InputEvent.NavigationDirection.Up => FindFirstOccupiedSpot(3, 2, 4, 1, 5, 0, 6),
                    InputEvent.NavigationDirection.Down => FindFirstOccupiedSpot(3, 4, 2, 5, 1, 6, 0),
                    InputEvent.NavigationDirection.Left => FindFirstOccupiedSpot(3, 2, 4, 1, 5, 0, 6, 7),
                    InputEvent.NavigationDirection.Right => FindFirstOccupiedSpot(3, 7, 0, 6, 1, 5, 2, 4),
                    _ => currentSpotIdx
                },
                4 => direction switch
                {
                    InputEvent.NavigationDirection.Up or
                    InputEvent.NavigationDirection.Down => FindFirstOccupiedSpot(4, 2, 3, 1, 0, 7),
                    InputEvent.NavigationDirection.Left => FindFirstOccupiedSpot(4, 5, 1, 6, 0, 7, 3),
                    InputEvent.NavigationDirection.Right => FindFirstOccupiedSpot(4, 3, 7, 0, 6, 1, 5),
                    _ => currentSpotIdx
                },
                5 => direction switch
                {
                    InputEvent.NavigationDirection.Up or
                    InputEvent.NavigationDirection.Down => FindFirstOccupiedSpot(5, 1, 0, 2, 7, 3),
                    InputEvent.NavigationDirection.Left => FindFirstOccupiedSpot(5, 6, 0, 7, 3, 4, 2),
                    InputEvent.NavigationDirection.Right => FindFirstOccupiedSpot(5, 4, 2, 3, 7, 6, 0),
                    _ => currentSpotIdx
                },
                6 => direction switch
                {
                    InputEvent.NavigationDirection.Up or
                    InputEvent.NavigationDirection.Down => FindFirstOccupiedSpot(6, 0, 7, 1, 2, 3),
                    InputEvent.NavigationDirection.Left => FindFirstOccupiedSpot(6, 7, 3, 4, 2, 5, 1),
                    InputEvent.NavigationDirection.Right => FindFirstOccupiedSpot(6, 5, 1, 4, 2, 3, 7),
                    _ => currentSpotIdx
                },
                7 => direction switch
                {
                    InputEvent.NavigationDirection.Up => FindFirstOccupiedSpot(7, 0, 6, 1, 5, 2, 4),
                    InputEvent.NavigationDirection.Down => FindFirstOccupiedSpot(7, 6, 0, 5, 1, 4, 2),
                    InputEvent.NavigationDirection.Left => FindFirstOccupiedSpot(7, 3, 2, 4, 1, 5, 0, 6),
                    InputEvent.NavigationDirection.Right => FindFirstOccupiedSpot(7, 0, 6, 1, 5, 2, 4, 3),
                    _ => currentSpotIdx
                },
                _ => currentSpotIdx,
            };
        }

        private int FindFirstOccupiedSpot(int fallback, params int[] indicesToCheck)
        {
            if (_moveInProgress)
                return indicesToCheck.First();

            foreach (var i in indicesToCheck)
            {
                if (_spots.First(x => x.SpotIndex == i).HasPlayerData != default)
                    return i;
            }
            return fallback;
        }
        #endregion
    }
}
