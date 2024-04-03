using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.Common;
using PokerTracker3000.Common.Messages;
using PokerTracker3000.GameSession;
using PokerTracker3000.Interfaces;

using InputEvent = PokerTracker3000.Input.UserInputEvent;

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
        private readonly MainWindowFocusManager _focusManager;
        private readonly IGameEventBus _eventBus;
#pragma warning disable IDE0052 // The reference needs to be kept alive
        private readonly AudioManager _audioManager;
#pragma warning restore IDE0052
        #endregion

        public MainWindowViewModel(IGameEventBus eventBus, ApplicationSettings settings, MainWindowFocusManager focusManager)
        {
            _focusManager = focusManager;
            _eventBus = eventBus;

            var clock = new GameClock(_eventBus);

            _audioManager = new AudioManager(eventBus, clock, settings.RiffSoundEffectPath);
            var gameSettings = new GameSettings();

            SessionManager = new(eventBus, gameSettings, focusManager, new GameStagesManager(_eventBus, clock, gameSettings), new(), clock, settings.DefaultPlayerImagePath);
            SideMenuViewModel = new(eventBus, focusManager, SessionManager);

            // SpotifyClientViewModel = new(_settings.ClientId, _settings.LocalHttpListenerPort, _settings.PkceAuthorizationVerifierLength);
            //Task.Run(async () => await SpotifyClientViewModel.AuthorizeApplication());

            _eventBus.RegisterListener(this, (t, m) => ApplicationClosing(m), GameEventBus.EventType.ApplicationClosing);
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

        public void ApplicationClosing(IInternalMessage m)
        {
            if (m is not ApplicationClosingMessage message)
                return;

            // TODO: Close Spotify connection if open

            message.NumberOfClosingCallbacksCalled++;
        }
    }
}
