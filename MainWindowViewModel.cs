﻿using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.Common;
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
        public bool _spotifyInfoOpen = false;
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

        public string ProgramDescription { get; init; } = string.Empty;

        public bool SpotifyInfoOpen
        {
            get => _spotifyInfoOpen;
            private set => SetProperty(ref _spotifyInfoOpen, value);
        }

        public GameSessionManager SessionManager { get; }

        public SideMenuViewModel SideMenuViewModel { get; }

        public SpotifyClientViewModel SpotifyViewModel { get; }
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
            SpotifyViewModel = new(focusManager, settings.ClientId, settings.LocalHttpListenerPort, settings.PkceAuthorizationVerifierLength, 3000);
            SideMenuViewModel = new(eventBus, focusManager, SessionManager, SpotifyViewModel);

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
                    SpotifyInfoOpen = _focusManager.CurrentFocusArea == MainWindowFocusManager.FocusArea.SpotifyInformationBox;
                }
            }
            else if (inputEvent.IsNavigationEvent)
            {
                _focusManager.HandleNavigationEvent(inputEvent.Direction);
            }
        }
    }
}
