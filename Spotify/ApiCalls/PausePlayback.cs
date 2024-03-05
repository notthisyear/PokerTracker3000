using System.Collections.Generic;

namespace PokerTracker3000.Spotify.ApiCalls
{
    internal sealed class PausePlayback : SpotifyApiCallBase
    {
        public override SpotifyApiCallType ApiCall => SpotifyApiCallType.PausePlayback;

        protected override SpotifyHttpClient.HttpRequestMethod RequestMethod => SpotifyHttpClient.HttpRequestMethod.Put;

        protected override List<AccessScopeType> Scopes => new() { AccessScopeType.UserModifyPlaybackState };

        protected override string Endpoint => SpotifyEndpoint.PlayerBaseEndpoint + "/pause";
    }
}
