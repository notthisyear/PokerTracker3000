using System;
using System.Collections.Generic;
using System.Net.Http;
using PokerTracker3000.Spotify.ApiCalls.Shared;

namespace PokerTracker3000.Spotify.ApiCalls
{
    internal sealed class StartOrResumePlayback : SpotifyApiCallBase
    {
        /// <summary>
        /// Optional. Spotify URI of the context to play. Valid contexts are albums, artists & playlists.
        /// </summary>
        public string? ContextUri { get; init; }

        /// <summary>
        /// Optional. A JSON array of the Spotify track URIs to play.
        /// </summary>
        public string[]? Uris { get; init; }

        /// <summary>
        /// Optional. Indicates from where in the context playback should start. Only available when context_uri corresponds to an album or playlist object.
        /// </summary>
        public OffsetObject Offset { get; init; }

        public override SpotifyApiCallType ApiCall => SpotifyApiCallType.StartOrResumePlayback;

        protected override SpotifyHttpClient.HttpRequestMethod RequestMethod => SpotifyHttpClient.HttpRequestMethod.Put;

        protected override List<AccessScopeType> Scopes => new() { AccessScopeType.UserModifyPlaybackState };

        protected override string Endpoint => SpotifyEndpoint.PlayerBaseEndpoint + "/play?device_id=" + _deviceId;

        private readonly string _deviceId;

        public StartOrResumePlayback(string deviceId)
        {
            _deviceId = deviceId;
        }
        protected override void AddBodyToRequestIfNeeded(HttpRequestMessage request)
        {
            var hasContextUri = !string.IsNullOrEmpty(ContextUri);
            var hasUris = Uris != default && Uris.Length > 0;

            if (hasContextUri || hasUris)
                throw new NotImplementedException("StartOrResumePlayback does not support adding content to the put request");
        }
    }
}
