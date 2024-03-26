using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.Common;
using PokerTracker3000.GameSession;

using InputEvent = PokerTracker3000.Input.InputManager.UserInputEvent;

namespace PokerTracker3000.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        #region Public properties

        #region Backing fields
        public bool _leftSideMenuOpen = false;
        public bool _rightSideMenuOpen = false;
        #endregion

        public bool LeftSideMenuOpen
        {
            get => _leftSideMenuOpen;
            private set => SetProperty(ref _leftSideMenuOpen, value);
        }

        public bool RightSideMenuOpen
        {
            get => _rightSideMenuOpen;
            private set => SetProperty(ref _rightSideMenuOpen, value);
        }

        public string ProgramDescription { get; init; } = "hello";

        public GameSessionManager SessionManager { get; }

        public SideMenuViewModel SideMenuViewModel { get; }

        // public SpotifyClientViewModel SpotifyClientViewModel { get; init; }
        #endregion

        #region Private fields
        private readonly ApplicationSettings _settings;
        private readonly MainWindowFocusManager _focusManager;
        #endregion

        public MainWindowViewModel(ApplicationSettings settings, MainWindowFocusManager focusManager)
        {
            _settings = settings;
            _focusManager = focusManager;

            SessionManager = new(settings.DefaultPlayerImagePath, _focusManager);
            SideMenuViewModel = new(focusManager, SessionManager);

            // SpotifyClientViewModel = new(_settings.ClientId, _settings.LocalHttpListenerPort, _settings.PkceAuthorizationVerifierLength);
            //Task.Run(async () => await SpotifyClientViewModel.AuthorizeApplication());

            // Navigate to the edit stage menu
            HandleInputEvent(new() { IsButtonEvent = true, Button = InputEvent.ButtonEventType.Start });
            HandleInputEvent(new() { IsNavigationEvent = true, Direction = InputEvent.NavigationDirection.Down });
            HandleInputEvent(new() { IsNavigationEvent = true, Direction = InputEvent.NavigationDirection.Down });
            HandleInputEvent(new() { IsNavigationEvent = true, Direction = InputEvent.NavigationDirection.Down });
            HandleInputEvent(new() { IsNavigationEvent = true, Direction = InputEvent.NavigationDirection.Down });
            HandleInputEvent(new() { IsButtonEvent = true, Button = InputEvent.ButtonEventType.Select });
            HandleInputEvent(new() { IsNavigationEvent = true, Direction = InputEvent.NavigationDirection.Down });
            HandleInputEvent(new() { IsButtonEvent = true, Button = InputEvent.ButtonEventType.Select });
            HandleInputEvent(new() { IsButtonEvent = true, Button = InputEvent.ButtonEventType.Select });
        }

        public void HandleInputEvent(InputEvent inputEvent)
        {
            if (inputEvent.IsButtonEvent)
            {
                if (inputEvent.Button == InputEvent.ButtonEventType.InfoButton)
                {
                    RightSideMenuOpen = !RightSideMenuOpen;
                }
                else
                {
                    _focusManager.HandleButtonPressedEvent(inputEvent.Button);
                    LeftSideMenuOpen = _focusManager.CurrentFocusArea == MainWindowFocusManager.FocusArea.LeftSideMenu ||
                        _focusManager.CurrentFocusArea == MainWindowFocusManager.FocusArea.SideMenuEditOption;
                }
            }
            else if (inputEvent.IsNavigationEvent)
            {
                _focusManager.HandleNavigationEvent(inputEvent.Direction);
            }
        }

        public void NotifyWindowClosed()
        {
            // TODO: Close Spotify connection if open
        }
    }
}
