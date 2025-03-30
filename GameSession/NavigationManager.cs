using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using InputEvent = PokerTracker3000.Input.UserInputEvent;

namespace PokerTracker3000.GameSession
{
    public class NavigationManager
    {
        public record Node(float X, float Y);

        private readonly struct Distance
        {
            public float XDifference { get; init; }

            public float YDifference { get; init; }

            public int StartSpotId { get; init; }

            public int EndSpotId { get; init; }

            public readonly double Total => Math.Sqrt(XDifference * XDifference + YDifference * YDifference);
        }

        private readonly struct ItemCoordinate
        {
            public int Id { get; init; }

            public float X { get; init; }

            public float Y { get; init; }

            private readonly Dictionary<InputEvent.NavigationDirection, List<int>> _navigationOrderForDirection;

            public ItemCoordinate()
            {
                _navigationOrderForDirection = [];
            }

            public void Initialize(ReadOnlyCollection<ItemCoordinate> allSpots)
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
                                .. distanceToOtherSpots.Where(x => x.XDifference > 0).Reverse().OrderBy(x => Math.Abs(x.YDifference)).Select(x => x.EndSpotId)],
                        InputEvent.NavigationDirection.Right =>
                                [.. distanceToOtherSpots.Where(x => x.XDifference > 0).Select(x => x.EndSpotId),
                                .. distanceToOtherSpots.Where(x => x.XDifference < 0).Reverse().OrderBy(x => Math.Abs(x.YDifference)).Select(x => x.EndSpotId)],
                        InputEvent.NavigationDirection.Up =>
                                [.. distanceToOtherSpots.Where(x => x.YDifference < 0).Select(x => x.EndSpotId),
                                .. distanceToOtherSpots.Where(x => x.YDifference > 0).Reverse().OrderBy(x => Math.Abs(x.XDifference)).Select(x => x.EndSpotId)],
                        InputEvent.NavigationDirection.Down =>
                                [.. distanceToOtherSpots.Where(x => x.YDifference > 0).Select(x => x.EndSpotId),
                                .. distanceToOtherSpots.Where(x => x.YDifference < 0).Reverse().OrderBy(x => Math.Abs(x.XDifference)).Select(x => x.EndSpotId)],
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
        private readonly Dictionary<TableLayout, Dictionary<int, ItemCoordinate>> _layoutNavigationMaps;
        private readonly Dictionary<int, Dictionary<int, ItemCoordinate>> _registeredNavigationMaps;
        #endregion

        public NavigationManager()
        {
            _layoutNavigationMaps = [];
            _registeredNavigationMaps = [];

            SetupNavigationInformationForPlayerLayouts();
        }

        #region Public methods
        public int RegisterNavigation(List<Node> nodes)
        {
            var nextId = _registeredNavigationMaps.Count;
            _registeredNavigationMaps.Add(nextId, []);
            for (var i = 0; i < nodes.Count; i++)
                _registeredNavigationMaps[nextId].Add(i, new() { Id = i, X = nodes[i].X, Y = nodes[i].Y });

            InitializeNavigationDictionary(_registeredNavigationMaps[nextId]);
            return nextId;
        }

        public int Navigate(TableLayout layout, int currentSpotIdx, InputEvent.NavigationDirection direction, Predicate<int>? spotValidation = default)
        {
            if (_layoutNavigationMaps.TryGetValue(layout, out var spotNavigationMap))
                return Navigate(spotNavigationMap, currentSpotIdx, direction, spotValidation);
            return currentSpotIdx;
        }

        public int Navigate(int layoutId, int currentIdx, InputEvent.NavigationDirection direction, Predicate<int>? nodeValidation = default)
        {
            if (_registeredNavigationMaps.TryGetValue(layoutId, out var navigationMap))
                return Navigate(navigationMap, currentIdx, direction, nodeValidation);
            return currentIdx;
        }
        #endregion

        #region Private methods
        private void SetupNavigationInformationForPlayerLayouts()
        {
            /* Layout:
            *
            * 0   1
            *
            */
            AddToTableLayoutNavigationList(TableLayout.TwoPlayers,
                [
                    new(X: 0, Y: 0),
                    new(X: 1, Y: 0.1f)
                ]);

            /* Layout:
            *
            *   0  1
            *
            *   3  2
            */
            AddToTableLayoutNavigationList(TableLayout.FourPlayers,
                [
                    new(X: 0, Y: 0),
                    new(X: 1, Y: 0),
                    new(X: 1, Y: 1),
                    new(X: 0, Y: 1)
                ]);

            /* Layout:
            *
            *   0  1  2
            *
            *   5  4  3
            */
            AddToTableLayoutNavigationList(TableLayout.SixPlayers,
                [
                    new(X: 0, Y: 0),
                    new(X: 1, Y: 0),
                    new(X: 2, Y: 0),
                    new(X: 0, Y: 1),
                    new(X: 1, Y: 1),
                    new(X: 2, Y: 1)
                ]);

            /* Layout:
            *
            *   0  1  2
            * 7         3
            *   6  5  4
            */
            AddToTableLayoutNavigationList(TableLayout.EightPlayers,
                [
                    new(X: 1, Y: 0),
                    new(X: 2, Y: 0),
                    new(X: 3, Y: 0),
                    new(X: 5.5F, Y: 1),
                    new(X: 3, Y: 2),
                    new(X: 2, Y: 2),
                    new(X: 1, Y: 2),
                    new(X: -1.5F, Y: 1)
                ]);

            /* Layout:
                *
                *   0  1  2
                * 9         3
                * 8         4
                *   7  6  5
                */
            AddToTableLayoutNavigationList(TableLayout.TenPlayers,
                [
                    new(X: 1, Y: 0),
                    new(X: 2, Y: 0),
                    new(X: 3, Y: 0),
                    new(X: 4, Y: 0.33F),
                    new(X: 4, Y: 0.66F),
                    new(X: 3, Y: 1),
                    new(X: 2, Y: 1),
                    new(X: 1, Y: 1),
                    new(X: 0, Y: 0.66F),
                    new(X: 0, Y: 0.33F)
                ]);

            /* Layout:
            *
            *    0  1  2
            *  11        3
            * 10          4
            *  9         5
            *    8  7  6
            */
            AddToTableLayoutNavigationList(TableLayout.TwelvePlayers,
                [
                    new(X: 1, Y: 0),
                    new(X: 2, Y: 0),
                    new(X: 3, Y: 0),
                    new(X: 3.75F, Y: 0.25F),
                    new(X: 4.5F, Y: 0.5F),
                    new(X: 3.75F, Y: 0.75F),
                    new(X: 3, Y: 1),
                    new(X: 2, Y: 1),
                    new(X: 1, Y: 1),
                    new(X: 0.25F, Y: 0.75F),
                    new(X: -0.5F, Y: 0.5F),
                    new(X: 0.25F, Y: 0.25F),
                ]);

            foreach (var layout in _layoutNavigationMaps.Keys)
                InitializeNavigationDictionary(_layoutNavigationMaps[layout]);
        }

        private void AddToTableLayoutNavigationList(TableLayout layout, List<Node> nodes)
        {
            if (_layoutNavigationMaps.ContainsKey(layout))
                throw new ArgumentException($"Key '{layout}' already exists", nameof(layout));

            _layoutNavigationMaps.Add(layout, []);
            for (var i = 0; i < nodes.Count; i++)
                _layoutNavigationMaps[layout].Add(i, new() { Id = i, X = nodes[i].X, Y = nodes[i].Y });
        }

        private static void InitializeNavigationDictionary(Dictionary<int, ItemCoordinate> navigationEntries)
        {
            foreach (var entry in navigationEntries.Values)
                entry.Initialize(navigationEntries.Values.ToList().AsReadOnly());
        }

        private static int Navigate(Dictionary<int, ItemCoordinate> navigationMap, int currentIdx, InputEvent.NavigationDirection direction, Predicate<int>? nodeValidation = default)
        {
            if (navigationMap.TryGetValue(currentIdx, out var coordinate) &&
                coordinate.TryGetValidMovementsInDirection(direction, out var navigationOrder))
            {
                return FindFirstAcceptedNode(currentIdx, nodeValidation, [.. navigationOrder!]);
            }
            return currentIdx;
        }

        private static int FindFirstAcceptedNode(int fallback, Predicate<int>? nodeValidation = default, params int[] indicesToCheck)
        {
            if (nodeValidation == default)
                return indicesToCheck.First();

            foreach (var i in indicesToCheck)
            {
                if (nodeValidation(i))
                    return i;
            }
            return fallback;
        }
        #endregion
    }
}
