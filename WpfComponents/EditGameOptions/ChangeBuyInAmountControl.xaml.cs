using System;
using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.GameSession;
using PokerTracker3000.Input;
using PokerTracker3000.Interfaces;

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
        public event EventHandler<InputManager.UserInputEvent.NavigationDirection>? Navigate;
        public event EventHandler<IInputRelay.ButtonEventArgs>? ButtonEvent;
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

            SessionManager.Navigate += (s, e) =>
            {
                if (SessionManager == default || SessionManager.CurrentGameEditOption != SideMenuViewModel.GameEditOption.ChangeDefaultBuyInAmount)
                    return;
                Navigate?.Invoke(this, e);
            };
            SessionManager.ButtonEvent += (s, e) =>
            {
                if (SessionManager == default || SessionManager.CurrentGameEditOption != SideMenuViewModel.GameEditOption.ChangeDefaultBuyInAmount)
                    return;

                if (e.ButtonEvent == InputManager.UserInputEvent.ButtonEventType.Select)
                {
                    editor.IsSelected = !editor.IsSelected;
                }
                else if (e.ButtonEvent == InputManager.UserInputEvent.ButtonEventType.GoBack)
                {
                    if (editor.IsSelected)
                    {
                        editor.IsSelected = false;
                        e.Handled = true;
                    }
                }
            };
        }
    }
}
