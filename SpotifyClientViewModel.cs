using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DotSpotifyWebWrapper;
using DotSpotifyWebWrapper.ApiCalls;
using DotSpotifyWebWrapper.ApiCalls.Shared;
using DotSpotifyWebWrapper.Types;

namespace PokerTracker3000
{
    public enum AuthenticationStatus
    {
        NotAuthenticated,
        Authenticated,
        AuthenticationFailed
    }

    public class SpotifyClientViewModel : ObservableObject, IDisposable
    {
        #region Public properties

        #region Backing fields
        private string _authenticationProgress = string.Empty;
        private AuthenticationStatus _authenticationStatus = AuthenticationStatus.NotAuthenticated;
        private string _authorizedUser = string.Empty;
        private bool _currentTrackIsPlaying = false;
        private string _currentTrackName = string.Empty;
        private string _currentTrackArtist = string.Empty;
        private string _currentTrackAlbum = string.Empty;
        private string _currentCoverArtPath = "C:\\Users\\Calle Lindquist\\Desktop\\tmp4q1klf.jpg";
        private int _currentTrackProgressSeconds;
        private int _currentTrackLengthSeconds;
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

        public string AuthorizedUser
        {
            get => _authorizedUser;
            private set => SetProperty(ref _authorizedUser, value);
        }

        public bool CurrentTrackIsPlaying
        {
            get => _currentTrackIsPlaying;
            private set => SetProperty(ref _currentTrackIsPlaying, value);
        }

        public string CurrentTrackName
        {
            get => _currentTrackName;
            private set => SetProperty(ref _currentTrackName, value);
        }

        public string CurrentTrackArtist
        {
            get => _currentTrackArtist;
            private set => SetProperty(ref _currentTrackArtist, value);
        }

        public string CurrentTrackAlbum
        {
            get => _currentTrackAlbum;
            private set => SetProperty(ref _currentTrackAlbum, value);
        }

        public string CurrentCoverArtPath
        {
            get => _currentCoverArtPath;
            private set => SetProperty(ref _currentCoverArtPath, value);
        }

        public int CurrentTrackProgressSeconds
        {
            get => _currentTrackProgressSeconds;
            private set => SetProperty(ref _currentTrackProgressSeconds, value);
        }

        public int CurrentTrackLengthSeconds
        {
            get => _currentTrackLengthSeconds;
            private set => SetProperty(ref _currentTrackLengthSeconds, value);
        }
        #endregion

        #region Private fields
        private readonly SpotifyClient _client;
        private readonly int _pkceVerifierLength;
        private readonly int _maxPlaybackQueryPeriod;
        private readonly Timer _queryStateTimer;
        private string _currentTrackId = string.Empty;
        private string _currentDeviceId = string.Empty;
        private int _currentDeviceVolume;
        private bool _disposedValue;
        #endregion

        public SpotifyClientViewModel(string clientId, int localListenerPort, int pkceVerifierLength, int maxPlaybackQueryPeriod)
        {
            _client = new(clientId, localListenerPort);
            _pkceVerifierLength = pkceVerifierLength;
            _maxPlaybackQueryPeriod = maxPlaybackQueryPeriod;
            _queryStateTimer = new((s) => QueryCurrentSongStateTimer(), default, Timeout.Infinite, Timeout.Infinite);
        }
        #region Public methods
        public async Task AuthorizeApplication()
        {
            if (AuthenticationStatus == AuthenticationStatus.Authenticated)
                return;

            var success = await _client.TryGetTokenForScopes(
                AuthorizationFlowType.AuthorizationCodePkce,
                default,
                _pkceVerifierLength,
                AccessScopeType.UserReadPrivate,
                AccessScopeType.UserReadEmail,
                AccessScopeType.UserReadPlaybackState,
                AccessScopeType.UserReadCurrentlyPlaying,
                AccessScopeType.UserModifyPlaybackState);

            AuthenticationStatus = success ? AuthenticationStatus.Authenticated : AuthenticationStatus.AuthenticationFailed;
        }

        public async Task TrySetUserName()
        {
            if (AuthenticationStatus != AuthenticationStatus.Authenticated)
                return;

            var call = new GetCurrentUserProfile();
            var result = await _client.SendSpotifyApiCall(call);

            if (result)
                AuthorizedUser = call.Response?.DisplayName ?? string.Empty;
        }

        public void StartTrackMonitoring()
        {
            if (AuthenticationStatus != AuthenticationStatus.Authenticated)
                return;

            _queryStateTimer.Change(Timeout.Infinite, Timeout.Infinite);
            QueryCurrentSongStateTimer();
        }

        //public async Task TryGetPlaybackState()
        //{
        //    if (AuthenticationStatus != AuthenticationStatus.Authenticated)
        //        return;

        //    var call = new GetPlaybackState();
        //    var result = await _client.SendSpotifyApiCall(call);
        //    var data = call.Response;

        //    if (data != default)
        //    {
        //        var deviceId = data.Device.Id;
        //        var pauseCall = new PausePlayback();
        //        result = await _client.SendSpotifyApiCall(pauseCall);

        //        var resumeCall = new StartOrResumePlayback(deviceId!);
        //        result = await _client.SendSpotifyApiCall(resumeCall);

        //        var setVolumeCall = new SetPlaybackVolume(50, deviceId!);
        //        result = await _client.SendSpotifyApiCall(setVolumeCall);
        //    }
        //}
        #endregion

        #region Private methods
        private async void QueryCurrentSongStateTimer()
        {
            var call = new GetPlaybackState();
            var t = _client.SendSpotifyApiCall(call);
            t.Wait();

            if (t.Result)
            {
                var data = call.Response;
                CurrentTrackIsPlaying = data?.IsPlaying ?? false;
                if (data != default && data.TrackOrEpisode.HasValue && CurrentTrackIsPlaying)
                {
                    _currentDeviceId = data.Device.Id ?? string.Empty;
                    _currentDeviceVolume = data.Device.VolumePercent ?? 0;
                    if (data.TrackOrEpisode.Value!.Is<TrackObject>())
                    {
                        var track = data.TrackOrEpisode.Value!.Get<TrackObject>()!;
                        if (CurrentTrackIsPlaying && !_currentTrackId.Equals(track.Id ?? string.Empty, StringComparison.InvariantCulture))
                        {
                            _currentTrackId = track.Id ?? string.Empty;
                            CurrentTrackName = track.Name ?? string.Empty;
                            CurrentTrackAlbum = track.Album.Name ?? string.Empty;
                            CurrentTrackLengthSeconds = (int)(track.DurationMs / 1000.0);
                            CurrentTrackProgressSeconds = (int)((data.ProgressMs ?? 0) / 1000.0);

                            if (track.Artists != default)
                            {
                                StringBuilder sb = new();
                                for (var i = 0; i < track.Artists.Length; i++)
                                {
                                    sb.Append(track.Artists[i].Name);
                                    if (i < track.Artists.Length - 1)
                                        sb.Append(", ");
                                }
                                CurrentTrackArtist = sb.ToString();
                            }

                            // Note: Typically, Spotify has three images in different size.
                            //        We try get the smallest one here.
                            if (track.Album.Images.Length > 0 && !string.IsNullOrEmpty(track.Album.Images.First().Url))
                            {
                                var coverArtPath = Path.GetTempFileName();
                                var (success, _) = await _client.DownloadImageAndSaveToFile(track.Album.Images.First().Url!, coverArtPath);
                                if (success)
                                    CurrentCoverArtPath = coverArtPath;
                            }
                        }
                    }
                }
            }

            _queryStateTimer.Change(_maxPlaybackQueryPeriod, Timeout.Infinite);
        }
        #endregion

        #region Disposal
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _queryStateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _queryStateTimer.Dispose();
                    _client.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
