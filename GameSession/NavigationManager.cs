using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using InputEvent = PokerTracker3000.Input.InputManager.UserInputEvent;

namespace PokerTracker3000.GameSession
{
    internal class NavigationManager
    {
        private readonly struct Distance
        {
            public float XDifference { get; init; }

            public float YDifference { get; init; }

            public int StartSpotId { get; init; }

            public int EndSpotId { get; init; }

            public readonly double Total => Math.Sqrt(XDifference * XDifference + YDifference * YDifference);
        }

        private readonly struct SpotCoordinate
        {
            public int Id { get; init; }

            public float X { get; init; }

            public float Y { get; init; }

            private readonly Dictionary<InputEvent.NavigationDirection, List<int>> _navigationOrderForDirection;

            public SpotCoordinate()
            {
                _navigationOrderForDirection = [];
            }

            public void Initialize(ReadOnlyCollection<SpotCoordinate> allSpots)
            {
                List<Distance> distanceToOtherSpots = [];
                foreach (var spot in allSpots)
                {
                    if (spot.Id == Id)
                        continue;

                    distanceToOtherSpots.Add(new()
                    {
                        XDifference = spot.X - X,
                        YDifference = spot.Y - Y,
                        StartSpotId = Id,
                        EndSpotId = spot.Id
                    });
                }

                distanceToOtherSpots.Sort((x, y) => x.Total < y.Total ? -1 : 1);

                foreach (InputEvent.NavigationDirection direction in Enum.GetValues(typeof(InputEvent.NavigationDirection)))
                {
                    List<int> navigationOrder = direction switch
                    {
                        InputEvent.NavigationDirection.Left =>
                                [.. distanceToOtherSpots.Where(x => x.XDifference < 0).Select(x => x.EndSpotId),
                                .. distanceToOtherSpots.Where(x => x.XDifference > 0).Select(x => x.EndSpotId).Reverse()],
                        InputEvent.NavigationDirection.Right =>
                                [.. distanceToOtherSpots.Where(x => x.XDifference > 0).Select(x => x.EndSpotId),
                                .. distanceToOtherSpots.Where(x => x.XDifference < 0).Select(x => x.EndSpotId).Reverse()],
                        InputEvent.NavigationDirection.Up =>
                                [.. distanceToOtherSpots.Where(x => x.YDifference < 0).Select(x => x.EndSpotId),
                                .. distanceToOtherSpots.Where(x => x.YDifference > 0).Select(x => x.EndSpotId)],
                        InputEvent.NavigationDirection.Down =>
                                [.. distanceToOtherSpots.Where(x => x.YDifference > 0).Select(x => x.EndSpotId),
                                .. distanceToOtherSpots.Where(x => x.YDifference < 0).Select(x => x.EndSpotId)],
                        _ => []
                    };

                    if (navigationOrder.Count > 0)
                        _navigationOrderForDirection.Add(direction, navigationOrder);
                }
            }

            public bool TryGetValidMovementsInDirection(InputEvent.NavigationDirection direction, out List<int>? navigationOrder)
                => _navigationOrderForDirection.TryGetValue(direction, out navigationOrder);
        }

        #region Private fields
        private readonly ReadOnlyCollection<PlayerSpot> _spots;
        private readonly Dictionary<TableLayout, Dictionary<int, SpotCoordinate>> _spotCoordinates;
        private bool _moveInProgress;
        #endregion

        public NavigationManager(ReadOnlyCollection<PlayerSpot> spots)
        {
            _spots = spots;
            _spotCoordinates = [];
            SetupNavigationInformation();
        }

        public int Navigate(TableLayout layout, int currentSpotIdx, InputEvent.NavigationDirection direction, bool moveInProgress)
        {
            _moveInProgress = moveInProgress;
            if (_spotCoordinates.TryGetValue(layout, out var spotCoordinates) &&
                spotCoordinates.TryGetValue(currentSpotIdx, out var coordinate) &&
                coordinate.TryGetValidMovementsInDirection(direction, out var navigationOrder))
            {
                return FindFirstOccupiedSpot(currentSpotIdx, [.. navigationOrder!]);
            }
            return currentSpotIdx;
        }

        #region Private methods
        private void SetupNavigationInformation()
        {
            /* Layout:
            *
            *   0
            *
            *   1
            */
            _spotCoordinates.Add(TableLayout.TwoPlayers,
                new()
                    {
                        { 0, new() { Id = 0, X = 0, Y = 0 } },
                        { 1, new() { Id = 1, X = 0, Y = 1 } },
                    });

            /* Layout:
            *
            *   0  1
            *
            *   3  2
            */
            _spotCoordinates.Add(TableLayout.FourPlayers,
                new()
                {
                    { 0, new() { Id = 0, X = 0, Y = 0 } },
                    { 1, new() { Id = 1, X = 1, Y = 0 } },
                    { 2, new() { Id = 2, X = 1, Y = 1 } },
                    { 3, new() { Id = 3, X = 0, Y = 1 } },
                });

            /* Layout:
            *
            *   0  1  2
            *
            *   5  4  3
            */
            _spotCoordinates.Add(TableLayout.SixPlayers,
                new()
                {
                    { 0, new() { Id = 0, X = 0, Y = 0 } },
                    { 1, new() { Id = 1, X = 1, Y = 0 } },
                    { 2, new() { Id = 2, X = 2, Y = 0 } },
                    { 3, new() { Id = 3, X = 0, Y = 1 } },
                    { 4, new() { Id = 4, X = 1, Y = 1 } },
                    { 5, new() { Id = 5, X = 2, Y = 1 } },
                });

            /* Layout:
            *
            *   0  1  2
            * 7         3
            *   6  5  4
            */
            _spotCoordinates.Add(TableLayout.EightPlayers,
                new()
                {
                    { 0, new() { Id = 0, X = 1, Y = 0 } },
                    { 1, new() { Id = 1, X = 2, Y = 0 } },
                    { 2, new() { Id = 2, X = 3, Y = 0 } },
                    { 3, new() { Id = 3, X = 4, Y = 0.5F } },
                    { 4, new() { Id = 4, X = 3, Y = 1 } },
                    { 5, new() { Id = 5, X = 2, Y = 1 } },
                    { 6, new() { Id = 6, X = 1, Y = 1 } },
                    { 7, new() { Id = 7, X = 0, Y = 0.5F } },
                });

            /* Layout:
                *
                *   0  1  2
                * 9         3
                * 8         4
                *   7  6  5
                */
            _spotCoordinates.Add(TableLayout.TenPlayers,
                new()
                {
                    { 0, new() { Id = 0, X = 1, Y = 0 } },
                    { 1, new() { Id = 1, X = 2, Y = 0 } },
                    { 2, new() { Id = 2, X = 3, Y = 0 } },
                    { 3, new() { Id = 3, X = 4, Y = 0.33F } },
                    { 4, new() { Id = 4, X = 4, Y = 0.66F } },
                    { 5, new() { Id = 5, X = 3, Y = 1 } },
                    { 6, new() { Id = 6, X = 2, Y = 1 } },
                    { 7, new() { Id = 7, X = 1, Y = 1 } },
                    { 8, new() { Id = 8, X = 0, Y = 0.33F } },
                    { 9, new() { Id = 9, X = 0, Y = 0.66F } },
                });

            /* Layout:
            *
            *    0  1  2
            *  11        3
            * 10          4
            *  9         5
            *    8  7  6
            */
            _spotCoordinates.Add(TableLayout.TwelvePlayers,
                new()
                {
                    { 0, new() { Id = 0, X = 1, Y = 0 } },
                    { 1, new() { Id = 1, X = 2, Y = 0 } },
                    { 2, new() { Id = 2, X = 3, Y = 0 } },
                    { 3, new() { Id = 3, X = 3.75F, Y = 0.25F } },
                    { 4, new() { Id = 4, X = 4.5F, Y = 0.5F } },
                    { 5, new() { Id = 5, X = 3.75F, Y = 0.75F } },
                    { 6, new() { Id = 6, X = 3, Y = 1 } },
                    { 7, new() { Id = 7, X = 2, Y = 1 } },
                    { 8, new() { Id = 8, X = 1, Y = 1 } },
                    { 9, new() { Id = 9, X = 0.25F, Y = 0.75F } },
                    { 10, new() { Id = 10, X = -0.5F, Y = 0.5F } },
                    { 11, new() { Id = 11, X = 0.25F, Y = 0.25F } },
                });

            foreach (var layout in _spotCoordinates)
            {
                foreach (var spot in _spotCoordinates[layout.Key].Values)
                    spot.Initialize(_spotCoordinates[layout.Key].Values.ToList().AsReadOnly());
            }
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
