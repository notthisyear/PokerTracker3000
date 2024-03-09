using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.GameSession;

using InputEvent = PokerTracker3000.Input.InputManager.UserInputEvent;

namespace PokerTracker3000.WpfComponents
{
    public partial class PokerTable : UserControl
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

        #region Dependency properties
        public GameSessionManager ViewModel
        {
            get => (GameSessionManager)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(GameSessionManager),
            typeof(PokerTable),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));

        #region Read-only dependency properties
        public TableLayout CurrentTableLayout
        {
            get => (TableLayout)GetValue(s_currentTableLayoutProperty);
            private set => SetValue(s_currentTableLayoutPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_currentTableLayoutPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(CurrentTableLayout),
            typeof(TableLayout),
            typeof(PokerTable),
            new FrameworkPropertyMetadata(TableLayout.TwoPlayers, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_currentTableLayoutProperty = s_currentTableLayoutPropertyKey.DependencyProperty;

        public PlayerSpot? SelectedSpot
        {
            get => (PlayerSpot?)GetValue(s_selectedSpotProperty);
            private set => SetValue(s_selectedSpotPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_selectedSpotPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(SelectedSpot),
            typeof(PlayerSpot),
            typeof(PokerTable),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_selectedSpotProperty = s_selectedSpotPropertyKey.DependencyProperty;

        public PlayerSpot? Spot1
        {
            get => (PlayerSpot?)GetValue(s_spot1Property);
            private set => SetValue(s_spot1PropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_spot1PropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Spot1),
            typeof(PlayerSpot),
            typeof(PokerTable),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_spot1Property = s_spot1PropertyKey.DependencyProperty;

        public PlayerSpot? Spot2
        {
            get => (PlayerSpot?)GetValue(s_spot2Property);
            private set => SetValue(s_spot2PropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_spot2PropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Spot2),
            typeof(PlayerSpot),
            typeof(PokerTable),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_spot2Property = s_spot2PropertyKey.DependencyProperty;

        public PlayerSpot? Spot3
        {
            get => (PlayerSpot?)GetValue(s_spot3Property);
            private set => SetValue(s_spot3PropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_spot3PropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Spot3),
            typeof(PlayerSpot),
            typeof(PokerTable),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_spot3Property = s_spot3PropertyKey.DependencyProperty;

        public PlayerSpot? Spot4
        {
            get => (PlayerSpot?)GetValue(s_spot4Property);
            private set => SetValue(s_spot4PropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_spot4PropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Spot4),
            typeof(PlayerSpot),
            typeof(PokerTable),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_spot4Property = s_spot4PropertyKey.DependencyProperty;

        public PlayerSpot? Spot5
        {
            get => (PlayerSpot?)GetValue(s_spot5Property);
            private set => SetValue(s_spot5PropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_spot5PropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Spot5),
            typeof(PlayerSpot),
            typeof(PokerTable),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_spot5Property = s_spot5PropertyKey.DependencyProperty;

        public PlayerSpot? Spot6
        {
            get => (PlayerSpot?)GetValue(s_spot6Property);
            private set => SetValue(s_spot6PropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_spot6PropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Spot6),
            typeof(PlayerSpot),
            typeof(PokerTable),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_spot6Property = s_spot6PropertyKey.DependencyProperty;

        public PlayerSpot? Spot7
        {
            get => (PlayerSpot?)GetValue(s_spot7Property);
            private set => SetValue(s_spot7PropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_spot7PropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Spot7),
            typeof(PlayerSpot),
            typeof(PokerTable),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_spot7Property = s_spot7PropertyKey.DependencyProperty;

        public PlayerSpot? Spot8
        {
            get => (PlayerSpot?)GetValue(s_spot8Property);
            private set => SetValue(s_spot8PropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_spot8PropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Spot8),
            typeof(PlayerSpot),
            typeof(PokerTable),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_spot8Property = s_spot8PropertyKey.DependencyProperty;
        #endregion

        #endregion

        #region Private fields
        private bool _isRunningInitialCheckOnNumberOfPlayers = false;
        private static readonly List<PlayerSpot> s_playerSpots = new();
        #endregion

        public PokerTable()
        {
            InitializeComponent();
            Loaded += PokerTableLoaded;
        }

        private void PokerTableLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= PokerTableLoaded;
            _isRunningInitialCheckOnNumberOfPlayers = true;
            InitializeSpotList();
            ViewModel.Players.CollectionChanged += NumberOfPlayersChanged;
            SetCurrentTableLayout();

            foreach (var player in ViewModel.Players)
                AddPlayerToSpot(player);

            _isRunningInitialCheckOnNumberOfPlayers = false;
        }

        private void InitializeSpotList()
        {
            Spot1 = new() { SpotIndex = 0 };
            Spot2 = new() { SpotIndex = 1 };
            Spot3 = new() { SpotIndex = 2 };
            Spot4 = new() { SpotIndex = 3 };
            Spot5 = new() { SpotIndex = 4 };
            Spot6 = new() { SpotIndex = 5 };
            Spot7 = new() { SpotIndex = 6 };
            Spot8 = new() { SpotIndex = 7 };

            s_playerSpots.Add(Spot1);
            s_playerSpots.Add(Spot2);
            s_playerSpots.Add(Spot3);
            s_playerSpots.Add(Spot4);
            s_playerSpots.Add(Spot5);
            s_playerSpots.Add(Spot6);
            s_playerSpots.Add(Spot7);
            s_playerSpots.Add(Spot8);

            MainWindowFocusManager.RegisterPlayerSpots(s_playerSpots);
            MainWindowFocusManager.RegisterSpotNavigationCallback((int currentSpotIdx, InputEvent.NavigationDirection direction) =>
            {
                switch (CurrentTableLayout)
                {
                    case TableLayout.TwoPlayers:
                        return currentSpotIdx == 0 ? 1 : 0;

                    case TableLayout.FourPlayers:
                        {
                            var onTopRow = currentSpotIdx == 0 || currentSpotIdx == 1;
                            return direction switch
                            {
                                InputEvent.NavigationDirection.Right or InputEvent.NavigationDirection.Left => ((currentSpotIdx + 1) % 2) + (onTopRow ? 0 : 2),
                                InputEvent.NavigationDirection.Up or InputEvent.NavigationDirection.Down => (currentSpotIdx + 2) % 4,
                                _ => currentSpotIdx
                            };
                        }
                    case TableLayout.SixPlayers:
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
                    case TableLayout.EightPlayers:
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
                    default:
                        return currentSpotIdx;
                }
            });
            MainWindowFocusManager.RegisterPlayerOptionsCallback((PlayerSpot activeSpot, InputEvent.NavigationDirection direction) => activeSpot.ChangeSelectedOption(direction));
            MainWindowFocusManager.RegisterPlayerInfoBoxSelectCallback((PlayerSpot activeSpot) =>
            {
                switch (activeSpot.GetSelectedOption().Option)
                {
                    case PlayerEditOption.EditOption.ChangeName:
                        SelectedSpot = activeSpot;
                        changeNameBox.Focus();
                        changeNameBox.Select(changeNameBox.Text.Length, 0);
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

                        activeSpot.Remove();
                        var newSpotIdx = activeSpot.SpotIndex + 1;
                        var newSpotToFocus = activeSpot;
                        while (!newSpotToFocus.HasPlayerData)
                            newSpotToFocus = s_playerSpots[newSpotIdx++ % s_playerSpots.Count];
                        newSpotToFocus.IsSelected = true;
                        return MainWindowFocusManager.FocusArea.Players;

                    case PlayerEditOption.EditOption.AddOn:
                    case PlayerEditOption.EditOption.BuyIn:
                    default:
                        return MainWindowFocusManager.FocusArea.None;
                };
            });
            MainWindowFocusManager.RegisterEditMenuLostFocusCallback(() =>
            {
                Application.Current.MainWindow.Focus();
                SelectedSpot = default;
            });
        }

        private void NumberOfPlayersChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isRunningInitialCheckOnNumberOfPlayers)
                return;

            SetCurrentTableLayout();
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != default)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is PlayerModel p)
                        AddPlayerToSpot(p);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != default)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is not PlayerModel p)
                        continue;

                    foreach (var spot in s_playerSpots)
                    {
                        if (spot != default && spot.IsPlayer(p.PlayerId))
                        {
                            spot.PlayerData = default;
                            break;
                        }
                    }
                }
            }
        }

        private void SetCurrentTableLayout()
        {
            if (ViewModel == default)
                return;

            CurrentTableLayout = ViewModel.Players.Count switch
            {
                > 10 => TableLayout.TwelvePlayers,
                > 8 => TableLayout.TenPlayers,
                > 6 => TableLayout.EightPlayers,
                > 4 => TableLayout.SixPlayers,
                > 2 => TableLayout.FourPlayers,
                _ => TableLayout.TwoPlayers,
            };
        }

        private static void AddPlayerToSpot(PlayerModel player)
        {
            foreach (var spot in s_playerSpots)
            {
                if (spot != default && !spot.HasPlayerData)
                {
                    spot.PlayerData = player;
                    break;
                }
            }
        }
    }
}
