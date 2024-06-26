﻿using System;
using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.GameSession;
using PokerTracker3000.Interfaces;

using InputEvent = PokerTracker3000.Input.UserInputEvent;

namespace PokerTracker3000.WpfComponents.EditGameOptions
{
    public partial class ChangeStageLengthControl : UserControl, IInputRelay
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
            typeof(ChangeStageLengthControl),
            new FrameworkPropertyMetadata(default));
        #endregion

        #region Events
        public event EventHandler<InputEvent.NavigationDirection>? Navigate;
        public event EventHandler<IInputRelay.ButtonEventArgs>? ButtonEvent;
        #endregion

        public ChangeStageLengthControl()
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
                    editor.IsSelected = SessionManager != default && SessionManager.CurrentGameEditOption == SideMenuViewModel.GameEditOption.ChangeDefaultStageLength;

            };
            SessionManager.Navigate += (s, e) =>
            {
                if (SessionManager == default || SessionManager.CurrentGameEditOption != SideMenuViewModel.GameEditOption.ChangeDefaultStageLength)
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
