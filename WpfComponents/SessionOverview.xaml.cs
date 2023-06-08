using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.GameSession;

namespace PokerTracker3000.WpfComponents
{
    public partial class SessionOverview : UserControl
    {
        #region Dependency properties
        public GameSessionManager SessionManager
        {
            get => (GameSessionManager)GetValue(SessionManagerProperty);
            set => SetValue(SessionManagerProperty, value);
        }
        public static readonly DependencyProperty SessionManagerProperty = DependencyProperty.Register(
            nameof(SessionManager),
            typeof(GameSessionManager),
            typeof(SessionOverview),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        public SessionOverview()
        {
            InitializeComponent();
        }
    }
}
