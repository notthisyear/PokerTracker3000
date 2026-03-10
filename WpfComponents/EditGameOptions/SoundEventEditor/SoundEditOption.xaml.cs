using System.Windows;
using System.Windows.Controls;

namespace PokerTracker3000.WpfComponents.EditGameOptions
{
    public partial class SoundEditOption : UserControl
    {
        #region Dependency properties
        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }
        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(
            nameof(DisplayName),
            typeof(string),
            typeof(SoundEditOption),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(SoundEditOption),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public int MaxTextLength
        {
            get { return (int)GetValue(MaxTextLengthProperty); }
            set { SetValue(MaxTextLengthProperty, value); }
        }
        public static readonly DependencyProperty MaxTextLengthProperty = DependencyProperty.Register(
            nameof(MaxTextLength),
            typeof(int),
            typeof(SoundEditOption),
            new FrameworkPropertyMetadata(35, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        public SoundEditOption()
        {
            InitializeComponent();
        }
    }
}

