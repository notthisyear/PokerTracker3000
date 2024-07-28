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

namespace PokerTracker3000.GameSession
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
        private string _authenticationProgress = "Link an account to show Spotify info";
        private AuthenticationStatus _authenticationStatus = AuthenticationStatus.NotAuthenticated;
        private bool _hasTrackInfo = false;
        private string _authorizedUser = string.Empty;
        private bool _currentTrackIsPlaying = false;
        private string _currentTrackName = string.Empty;
        private string _currentTrackArtist = string.Empty;
        private string _currentTrackAlbum = string.Empty;
        private string _currentCoverArtPath = string.Empty;
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

        public bool HasTrackInfo
        {
            get => _hasTrackInfo;
            private set => SetProperty(ref _hasTrackInfo, value);
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
        private readonly long _maxPlaybackQueryPeriodMs;
        private readonly Timer _queryStateTimer;

        private long _nextServerQueryMs = 0L;
        private long _lastQueryMs = 0L;
        private int _progressAtLastFetchMs = 0;

        private string _currentTrackId = string.Empty;
        private string _currentDeviceId = string.Empty;
        private int _currentDeviceVolume;

        private bool _disposedValue;
        private const int MillisecondsPerSecond = 1000;
        #endregion

        public SpotifyClientViewModel(string clientId, int localListenerPort, int pkceVerifierLength, int maxPlaybackQueryPeriodMs)
        {
            _client = new(clientId, localListenerPort, true);
            _pkceVerifierLength = pkceVerifierLength;
            _maxPlaybackQueryPeriodMs = maxPlaybackQueryPeriodMs;
            _queryStateTimer = new(async (s) => await QueryCurrentSongState(), default, Timeout.Infinite, Timeout.Infinite);

            if (_client.HasAccessToken)
            {
                Task.Run(async () =>
                {
                    await AuthorizeApplication();
                    if (AuthenticationStatus == AuthenticationStatus.Authenticated)
                    {
                        await TrySetUserName();
                        StartTrackMonitoring();
                    }
                });
            }
        }

        #region Public methods
        public async Task AuthorizeApplication()
        {
            if (AuthenticationStatus == AuthenticationStatus.Authenticated)
                return;

            var success = await _client.TryGetTokenForScopes(
                AuthorizationFlowType.AuthorizationCodePkce,
                new Progress<string>(s => AuthenticationProgress = s),
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

            _queryStateTimer.Change(10, Timeout.Infinite);
        }

        #endregion

        #region Private methods
        private async Task QueryCurrentSongState()
        {
            var msCount = Environment.TickCount & int.MaxValue;
            if (msCount < _nextServerQueryMs)
            {
                var msPassed = msCount - _lastQueryMs;
                var currentProgressMs = (int)(_progressAtLastFetchMs + msPassed);
                SetTrackProgress(currentProgressMs);
            }
            else
            {
                await QuerySongState();
            }

            if (!_disposedValue)
                _queryStateTimer.Change(500, Timeout.Infinite);
        }

        private async Task QuerySongState()
        {
            var call = new GetPlaybackState();
            var result = await _client.SendSpotifyApiCall(call);

            if (result)
            {
                _lastQueryMs = Environment.TickCount & int.MaxValue;
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
                            SetBasicTrackInfo(track);
                            CurrentCoverArtPath = string.Empty;
                        }

                        SetTrackProgress(data.ProgressMs ?? 0);
                        _progressAtLastFetchMs = data.ProgressMs ?? 0;

                        _nextServerQueryMs = Math.Min(_lastQueryMs + _maxPlaybackQueryPeriodMs,
                            _lastQueryMs + (CurrentTrackLengthSeconds - CurrentTrackProgressSeconds) * MillisecondsPerSecond);

                        if (string.IsNullOrEmpty(CurrentCoverArtPath))
                            CurrentCoverArtPath = await GetAlbumArt(track);
                    }
                    else
                    {
                        // TODO: Deal with EpisodeObject
                    }
                }
            }

            HasTrackInfo = !string.IsNullOrEmpty(CurrentTrackName);
        }

        private void SetBasicTrackInfo(TrackObject track)
        {
            _currentTrackId = track.Id ?? string.Empty;
            CurrentTrackName = track.Name ?? string.Empty;
            CurrentTrackAlbum = track.Album.Name ?? string.Empty;
            CurrentTrackLengthSeconds = (int)(track.DurationMs / 1000.0);

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
        }

        private void SetTrackProgress(int progressMs)
        {
            CurrentTrackProgressSeconds = progressMs / 1000;
        }

        private async Task<string> GetAlbumArt(TrackObject track)
        {
            // Note: Typically, Spotify has images in several sizes.
            //       We try get the largest one here.
            if (track.Album.Images.Length == 0 || string.IsNullOrEmpty(track.Album.Images.First().Url))
                return string.Empty;

            var coverArtPath = Path.GetTempFileName();
            var (success, _) = await _client.DownloadImageAndSaveToFile(track.Album.Images.First().Url!, coverArtPath);
            return success ? coverArtPath : string.Empty;
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
