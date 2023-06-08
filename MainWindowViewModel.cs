using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.Common;
using PokerTracker3000.GameSession;
using PokerTracker3000.Input;

namespace PokerTracker3000.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        #region Public properties

        #region Backing fields
        public bool _showStartMenu = false;
        #endregion

        public bool ShowStartMenu
        {
            get => _showStartMenu;
            private set => SetProperty(ref _showStartMenu, value);
        }

        public string ProgramDescription { get; init; } = "hello";

        public GameSessionManager SessionManager { get; }

        public InputManager InputManager { get; }

        public SpotifyClientViewModel  SpotifyClientViewModel { get; init; }
        #endregion

        private readonly ApplicationSettings _settings;

        public MainWindowViewModel(ApplicationSettings settings)
        {
            _settings = settings;
            InputManager = new();
            SessionManager = new(settings.DefaultPlayerImagePath);

            SpotifyClientViewModel = new(_settings.ClientId, _settings.LocalHttpListenerPort, _settings.PkceAuthorizationVerifierLength);
            //Task.Run(SpotifyClientViewModel.AuthorizeApplication);
        }

        public void NotifyWindowClosed()
        {
        }
    }
}
