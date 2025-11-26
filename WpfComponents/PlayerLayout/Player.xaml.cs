using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.GameSession;

namespace PokerTracker3000.WpfComponents.PlayerLayout
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
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public double MinImageWidth
        {
            get => (double)GetValue(MinImageWidthProperty);
            set => SetValue(MinImageWidthProperty, value);
        }
        public static readonly DependencyProperty MinImageWidthProperty = DependencyProperty.Register(
            nameof(MinImageWidth),
            typeof(double),
            typeof(Player),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));


        public double MaxImageWidth
        {
            get => (double)GetValue(MaxImageWidthProperty);
            set => SetValue(MaxImageWidthProperty, value);
        }
        public static readonly DependencyProperty MaxImageWidthProperty = DependencyProperty.Register(
            nameof(MaxImageWidth),
            typeof(double),
            typeof(Player),
            new FrameworkPropertyMetadata(double.PositiveInfinity, FrameworkPropertyMetadataOptions.AffectsRender));


        public Player()
        {
            InitializeComponent();
        }
    }
}
