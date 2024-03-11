using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.GameSession;

namespace PokerTracker3000.WpfComponents
{
    public partial class SideMenuOption : UserControl
    {
        #region Dependency properties
        public SideMenuViewModel? MenuViewModel
        {
            get => (SideMenuViewModel)GetValue(MenuViewModelProperty);
            set => SetValue(MenuViewModelProperty, value);
        }
        public static readonly DependencyProperty MenuViewModelProperty = DependencyProperty.Register(
            nameof(MenuViewModel),
            typeof(SideMenuViewModel),
            typeof(SideMenuOption),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));

        public SideMenuOptionModel? OptionModel
        {
            get => (SideMenuOptionModel)GetValue(OptionModelProperty);
            set => SetValue(OptionModelProperty, value);
        }
        public static readonly DependencyProperty OptionModelProperty = DependencyProperty.Register(
            nameof(OptionModel),
            typeof(SideMenuOptionModel),
            typeof(SideMenuOption),
            new FrameworkPropertyMetadata(default));
        #endregion

        public SideMenuOption()
        {
            InitializeComponent();
        }
    }
}
