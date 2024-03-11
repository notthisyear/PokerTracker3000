using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.GameSession;

namespace PokerTracker3000.WpfComponents
{
    public partial class PokerTable : UserControl
    {
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

        public PlayerSpot? Spot9
        {
            get => (PlayerSpot?)GetValue(s_spot9Property);
            private set => SetValue(s_spot9PropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_spot9PropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Spot9),
            typeof(PlayerSpot),
            typeof(PokerTable),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_spot9Property = s_spot9PropertyKey.DependencyProperty;

        public PlayerSpot? Spot10
        {
            get => (PlayerSpot?)GetValue(s_spot10Property);
            private set => SetValue(s_spot10PropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_spot10PropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Spot10),
            typeof(PlayerSpot),
            typeof(PokerTable),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_spot10Property = s_spot10PropertyKey.DependencyProperty;

        public PlayerSpot? Spot11
        {
            get => (PlayerSpot?)GetValue(s_spot11Property);
            private set => SetValue(s_spot11PropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_spot11PropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Spot11),
            typeof(PlayerSpot),
            typeof(PokerTable),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_spot11Property = s_spot11PropertyKey.DependencyProperty;

        public PlayerSpot? Spot12
        {
            get => (PlayerSpot?)GetValue(s_spot12Property);
            private set => SetValue(s_spot12PropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_spot12PropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Spot12),
            typeof(PlayerSpot),
            typeof(PokerTable),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsArrange));
        private static readonly DependencyProperty s_spot12Property = s_spot12PropertyKey.DependencyProperty;
        #endregion

        #endregion

        public PokerTable()
        {
            InitializeComponent();
            Loaded += PokerTableLoaded;
        }

        private void PokerTableLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= PokerTableLoaded;
            InitializeSpotList();
        }

        private void InitializeSpotList()
        {
            if (ViewModel == default)
                return;

            Spot1 = ViewModel.PlayerSpots[0];
            Spot2 = ViewModel.PlayerSpots[1];
            Spot3 = ViewModel.PlayerSpots[2];
            Spot4 = ViewModel.PlayerSpots[3];
            Spot5 = ViewModel.PlayerSpots[4];
            Spot6 = ViewModel.PlayerSpots[5];
            Spot7 = ViewModel.PlayerSpots[6];
            Spot8 = ViewModel.PlayerSpots[7];
            Spot9 = ViewModel.PlayerSpots[8];
            Spot10 = ViewModel.PlayerSpots[9];
            Spot11 = ViewModel.PlayerSpots[10];
            Spot12 = ViewModel.PlayerSpots[11];

            CalculateLayout(ViewModel.PlayerSpots.Where(x => x.HasPlayerData).Count());
            ViewModel.LayoutMightHaveChangedEvent += (s, e) => CalculateLayout(e);
            ViewModel.FocusManager.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName?.Equals(nameof(ViewModel.FocusManager.CurrentFocusArea), StringComparison.InvariantCulture) ?? false)
                {
                    if (ViewModel.FocusManager.CurrentFocusArea == MainWindowFocusManager.FocusArea.EditNameBox)
                    {
                        changeNameBox.Focus();
                        changeNameBox.Select(changeNameBox.Text.Length, 0);
                    }
                }
            };
        }

        private void CalculateLayout(int numberOfActivePlayers)
        {
            CurrentTableLayout = numberOfActivePlayers switch
            {
                > 10 => TableLayout.TwelvePlayers,
                > 8 => TableLayout.TenPlayers,
                > 6 => TableLayout.EightPlayers,
                > 4 => TableLayout.SixPlayers,
                > 2 => TableLayout.FourPlayers,
                _ => TableLayout.TwoPlayers,
            };

            if (ViewModel != default)
                ViewModel.SetTableLayout(CurrentTableLayout);
        }
    }
}
