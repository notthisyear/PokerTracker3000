using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.GameSession;

namespace PokerTracker3000.WpfComponents
{
    public partial class Player : UserControl
    {
        public enum SpotAlignment
        {
            TopLeft,
            TopCenter,
            TopRight,
            BottomLeft,
            BottomCenter,
            BottomRight,
            Left,
            Right
        };

        public PlayerSpot SpotData
        {
            get => (PlayerSpot)GetValue(SpotDataProperty);
            set => SetValue(SpotDataProperty, value);
        }
        public static readonly DependencyProperty SpotDataProperty = DependencyProperty.Register(
            nameof(SpotData),
            typeof(PlayerSpot),
            typeof(Player),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));

        public SpotAlignment Alignment
        {
            get => (SpotAlignment)GetValue(AlignmentProperty);
            set => SetValue(AlignmentProperty, value);
        }
        public static readonly DependencyProperty AlignmentProperty = DependencyProperty.Register(
            nameof(Alignment),
            typeof(SpotAlignment),
            typeof(Player),
            new FrameworkPropertyMetadata(SpotAlignment.TopCenter, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool AlwaysShowPlayerImage
        {
            get => (bool)GetValue(AlwaysShowPlayerImageProperty);
            set => SetValue(AlwaysShowPlayerImageProperty, value);
        }
        public static readonly DependencyProperty AlwaysShowPlayerImageProperty = DependencyProperty.Register(
            nameof(AlwaysShowPlayerImage),
            typeof(bool),
            typeof(Player),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public Player()
        {
            InitializeComponent();
        }
    }
}
