using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.GameSession;

namespace PokerTracker3000.WpfComponents
{
    public partial class EditPlayerOptionBox : UserControl
    {
        #region Dependency properties
        public PlayerEditOption OptionModel
        {
            get => (PlayerEditOption)GetValue(OptionModelProperty);
            set => SetValue(OptionModelProperty, value);
        }
        public static readonly DependencyProperty OptionModelProperty = DependencyProperty.Register(
            nameof(OptionModel),
            typeof(PlayerEditOption),
            typeof(EditPlayerOptionBox),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        public EditPlayerOptionBox()
        {
            InitializeComponent();
        }
    }
}
