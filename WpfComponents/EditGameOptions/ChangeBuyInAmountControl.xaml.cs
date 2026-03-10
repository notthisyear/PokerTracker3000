using System;
using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.GameSession;
using PokerTracker3000.Interfaces;
using ButtonEventArgs = PokerTracker3000.Interfaces.IInputRelay.ButtonEventArgs;
using NavigationEventArgs = PokerTracker3000.Interfaces.IInputRelay.NavigationEventArgs;

namespace PokerTracker3000.WpfComponents.EditGameOptions
{
    public partial class ChangeBuyInAmountControl : UserControl, IInputRelay
    {
        #region Dependency properties
        public GameSessionManager SessionManager
        {
            get { return (GameSessionManager)GetValue(SessionManagerProperty); }
            set { SetValue(SessionManagerProperty, value); }
        }
        public static readonly DependencyProperty SessionManagerProperty = DependencyProperty.Register(
            nameof(SessionManager),
            typeof(GameSessionManager),
            typeof(ChangeBuyInAmountControl),
            new FrameworkPropertyMetadata(default));
        #endregion

        #region Events
        public event EventHandler<NavigationEventArgs>? Navigate;
        public event EventHandler<ButtonEventArgs>? ButtonEvent { add { } remove { } }
        #endregion

        public ChangeBuyInAmountControl()
        {
            InitializeComponent();
            Loaded += ControlLoadedEvent;
        }

        private void ControlLoadedEvent(object sender, RoutedEventArgs e)
        {
            Loaded -= ControlLoadedEvent;
            if (SessionManager == default)
                return;

            SessionManager.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName?.Equals(nameof(SessionManager.CurrentGameEditOption), StringComparison.InvariantCulture) ?? false)
                    editor.IsSelected = SessionManager != default && SessionManager.CurrentGameEditOption == SideMenuViewModel.GameEditOption.ChangeDefaultBuyInAmount;

            };
            SessionManager.Navigate += (s, e) =>
            {
                if (SessionManager == default || SessionManager.CurrentGameEditOption != SideMenuViewModel.GameEditOption.ChangeDefaultBuyInAmount)
                    return;
                Navigate?.Invoke(this, e);
            };
        }

        private void EditorLoaded(object sender, RoutedEventArgs e)
        {
            editor.IsSelected = true;
        }
    }
}
