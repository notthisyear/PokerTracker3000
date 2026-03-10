using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.GameSession;

namespace PokerTracker3000.WpfComponents
{
    public partial class ButtonOptionBox : UserControl
    {
        #region Dependency properties
        public ButtonOptionModel OptionModel
        {
            get => (ButtonOptionModel)GetValue(OptionModelProperty);
            set => SetValue(OptionModelProperty, value);
        }
        public static readonly DependencyProperty OptionModelProperty = DependencyProperty.Register(
            nameof(OptionModel),
            typeof(ButtonOptionModel),
            typeof(ButtonOptionBox),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        public ButtonOptionBox()
        {
            InitializeComponent();
        }
    }
}
