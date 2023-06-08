using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.Spotify;

namespace PokerTracker3000
{
    public enum AuthenticationStatus
    {
        NotAuthenticated,
        Authenticated,
        AuthenticationFailed
    }

    public class SpotifyClientViewModel : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private string _authenticationProgress = string.Empty;
        private AuthenticationStatus _authenticationStatus = AuthenticationStatus.NotAuthenticated;
        #endregion

        public string AuthenticationProgress
        {
            get => _authenticationProgress;
            private set => SetProperty(ref _authenticationProgress, value);
        }

        public AuthenticationStatus AuthenticationStatus
        {
            get => _authenticationStatus;
            private set => SetProperty(ref _authenticationStatus, value);
        }
        #endregion

        #region Private fields
        private readonly SpotifyClient _client;
        private readonly int _pkceVerifierLength;
        #endregion

        public SpotifyClientViewModel(string clientId, int localListenerPort, int pkceVerifierLength)
        {
            _client = new(clientId, localListenerPort);
            _pkceVerifierLength = pkceVerifierLength;
        }

        #region Public methods
        public async Task AuthorizeApplication()
        {
            if (AuthenticationStatus == AuthenticationStatus.Authenticated)
                return;

            var success = await _client.TryAuthorize(Authorizer.AuthorizationFlowType.AuthorizationCodePkce, new Progress<string>(s => AuthenticationProgress = s), _pkceVerifierLength);
            AuthenticationStatus = success ? AuthenticationStatus.Authenticated : AuthenticationStatus.AuthenticationFailed;
        }
        #endregion
    }
}
