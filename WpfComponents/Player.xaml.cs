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

        public bool IsHighlighted
        {
            get => (bool)GetValue(IsHighlightedProperty);
            set => SetValue(IsHighlightedProperty, value);
        }
        public static readonly DependencyProperty IsHighlightedProperty = DependencyProperty.Register(
            nameof(IsHighlighted),
            typeof(bool),
            typeof(Player),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(Player),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public Player()
        {
            InitializeComponent();
        }
    }
}
