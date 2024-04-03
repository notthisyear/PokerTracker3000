using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SpotifyApiWrapper;
using SpotifyApiWrapper.ApiCalls;
using SpotifyApiWrapper.Types;

namespace PokerTracker3000
{
    public enum AuthenticationStatus
    {
        NotAuthenticated,
        Authenticated,
        AuthenticationFailed
    }

    public class SpotifyClientViewModel(string clientId, int localListenerPort, int pkceVerifierLength) : ObservableObject
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
        private readonly SpotifyClient _client = new(clientId, localListenerPort);
        private readonly int _pkceVerifierLength = pkceVerifierLength;

        #endregion

        #region Public methods
        public async Task AuthorizeApplication()
        {
            if (AuthenticationStatus == AuthenticationStatus.Authenticated)
                return;

            var success = await _client.TryGetTokenForScopes(
                AuthorizationFlowType.AuthorizationCodePkce,
                new Progress<string>(s =>
                {
                    AuthenticationProgress = s;
                    Debug.WriteLine(s);
                }),
                _pkceVerifierLength,
                AccessScopeType.UserReadPrivate,
                AccessScopeType.UserReadEmail,
                AccessScopeType.UserReadPlaybackState,
                AccessScopeType.UserReadCurrentlyPlaying,
                AccessScopeType.UserModifyPlaybackState);

            AuthenticationStatus = success ? AuthenticationStatus.Authenticated : AuthenticationStatus.AuthenticationFailed;
        }

        public async Task<string> TryGetUserName()
        {
            if (AuthenticationStatus != AuthenticationStatus.Authenticated)
                return string.Empty;

            var call = new GetCurrentUserProfile();
            var result = await _client.SendSpotifyApiCall(call);

            if (!result)
                return string.Empty;

            return call.Response!.DisplayName ?? string.Empty;
        }

        public async Task TryGetPlaybackState()
        {
            if (AuthenticationStatus != AuthenticationStatus.Authenticated)
                return;

            var call = new GetPlaybackState();
            var result = await _client.SendSpotifyApiCall(call);
            var data = call.Response;

            if (data != default)
            {
                var deviceId = data.Device.Id;
                //var pauseCall = new PausePlayback();
                //result = await _client.SendSpotifyApiCall(pauseCall);

                //var resumeCall = new StartOrResumePlayback(deviceId!);
                //result = await _client.SendSpotifyApiCall(resumeCall);

                var setVolumeCall = new SetPlaybackVolume(50, deviceId!);
                result = await _client.SendSpotifyApiCall(setVolumeCall);
            }
        }
        #endregion
    }
}
