using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PokerTracker3000.WpfComponents
{
    public partial class PokerChip : UserControl
    {
        public Brush MainColor
        {
            get => (Brush)GetValue(MainColorProperty);
            set => SetValue(MainColorProperty, value);
        }
        public static readonly DependencyProperty MainColorProperty = DependencyProperty.Register(
            nameof(MainColor),
            typeof(Brush),
            typeof(PokerChip),
            new FrameworkPropertyMetadata(Brushes.Yellow, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush AccentColor
        {
            get => (Brush)GetValue(AccentColorProperty);
            set => SetValue(AccentColorProperty, value);
        }
        public static readonly DependencyProperty AccentColorProperty = DependencyProperty.Register(
            nameof(AccentColor),
            typeof(Brush),
            typeof(PokerChip),
            new FrameworkPropertyMetadata(Brushes.Blue, FrameworkPropertyMetadataOptions.AffectsRender));

        public decimal ChipValue
        {
            get => (decimal)GetValue(ChipValueProperty);
            set => SetValue(ChipValueProperty, value);
        }
        public static readonly DependencyProperty ChipValueProperty = DependencyProperty.Register(
            nameof(ChipValue),
            typeof(decimal),
            typeof(PokerChip),
            new FrameworkPropertyMetadata((decimal)10, FrameworkPropertyMetadataOptions.AffectsRender));

        public PokerChip()
        {
            InitializeComponent();
        }
    }
}
