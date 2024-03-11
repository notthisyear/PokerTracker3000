using System.Windows;
using System.Windows.Controls;

namespace PokerTracker3000.WpfComponents
{
    public partial class SideMenu : UserControl
    {
        public SideMenuViewModel ViewModel
        {
            get => (SideMenuViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(SideMenuViewModel),
            typeof(SideMenu),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));

        public SideMenu()
        {
            InitializeComponent();
        }
    }
}
