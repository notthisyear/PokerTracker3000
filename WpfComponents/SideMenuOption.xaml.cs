using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.GameSession;

namespace PokerTracker3000.WpfComponents
{
    public partial class SideMenuOption : UserControl
    {
        #region Dependency properties
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
