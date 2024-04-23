using System.Windows;
using System.Windows.Controls;

namespace PokerTracker3000.WpfComponents
{
    public partial class SpotifyInformationBox : UserControl
    {
        #region Dependency properties
        public SpotifyClientViewModel ViewModel
        {
            get { return (SpotifyClientViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel),
            typeof(SpotifyClientViewModel),
            typeof(SpotifyInformationBox),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        public SpotifyInformationBox()
        {
            InitializeComponent();
        }
    }
}
