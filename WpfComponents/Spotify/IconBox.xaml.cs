using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PokerTracker3000.WpfComponents
{
    public partial class IconBox : UserControl
    {
        public Brush FillColor
        {
            get => (Brush)GetValue(FillColorProperty);
            set => SetValue(FillColorProperty, value);
        }
        public static readonly DependencyProperty FillColorProperty = DependencyProperty.Register(
            nameof(FillColor),
            typeof(Brush),
            typeof(IconBox),
            new FrameworkPropertyMetadata(new SolidColorBrush(new() { A = 150, R = 255, G = 255, B = 255 }),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush StrokeColor
        {
            get => (Brush)GetValue(StrokeColorProperty);
            set => SetValue(StrokeColorProperty, value);
        }
        public static readonly DependencyProperty StrokeColorProperty = DependencyProperty.Register(
            nameof(StrokeColor),
            typeof(Brush),
            typeof(IconBox),
            new FrameworkPropertyMetadata(new SolidColorBrush(new() { A = 255, R = 0, G = 0, B = 0 }),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            nameof(StrokeThickness),
            typeof(double),
            typeof(IconBox),
            new FrameworkPropertyMetadata(1.0,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public double ScaleFactor
        {
            get => (double)GetValue(ScaleFactorProperty);
            set => SetValue(ScaleFactorProperty, value);
        }
        public static readonly DependencyProperty ScaleFactorProperty = DependencyProperty.Register(
            nameof(ScaleFactor),
            typeof(double),
            typeof(IconBox),
            new FrameworkPropertyMetadata(1.0,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public IconType IconType
        {
            get => (IconType)GetValue(IconTypeProperty);
            set => SetValue(IconTypeProperty, value);
        }
        public static readonly DependencyProperty IconTypeProperty = DependencyProperty.Register(
            nameof(IconType),
            typeof(IconType),
            typeof(IconBox),
            new FrameworkPropertyMetadata(IconType.None, FrameworkPropertyMetadataOptions.AffectsRender, IconTypeChanged));

        private static void IconTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IconBox icon && e.NewValue is IconType newType && e.OldValue is IconType oldType && newType != oldType)
                icon.PathData = IconDataProvider.GetDataForIcon(newType);
        }

        public Geometry PathData
        {
            get => (Geometry)GetValue(s_pathDataProperty);
            private set => SetValue(s_pathDataPropertyKey, value);
        }
        private static readonly DependencyPropertyKey s_pathDataPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(PathData),
            typeof(Geometry),
            typeof(IconBox),
            new FrameworkPropertyMetadata(IconDataProvider.GetDataForIcon(IconType.None), FrameworkPropertyMetadataOptions.AffectsRender));
        private static readonly DependencyProperty s_pathDataProperty = s_pathDataPropertyKey.DependencyProperty;

        public IconBox()
        {
            InitializeComponent();
        }
    }
}
