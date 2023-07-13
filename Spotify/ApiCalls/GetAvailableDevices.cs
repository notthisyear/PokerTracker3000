using System.Collections.Generic;
using System.Threading.Tasks;
using PokerTracker3000.Spotify.ApiCalls.Shared;

namespace PokerTracker3000.Spotify.ApiCalls
{
    internal sealed class GetAvailableDevices : SpotifyApiCallBase
    {
        public record ResponseData
        {
            /// <summary>
            /// Array of DeviceObject.
            /// </summary>
            public List<DeviceObject>? Devices { get; init; }
        }

        public ResponseData? Response { get; private set; }

        public override SpotifyApiCallType ApiCall => SpotifyApiCallType.GetAvailableDevices;

        protected override SpotifyHttpClient.HttpRequestMethod RequestMethod => SpotifyHttpClient.HttpRequestMethod.Get;

        protected override List<AccessScopeType> Scopes => new() { AccessScopeType.UserReadPlaybackState };

        protected override string Endpoint => SpotifyEndpoint.PlayerBaseEndpoint + "/devices";

        protected override async Task ParseResponse()
        {
            var (success, iaEmpty, data) = await ReadAndDeserializeJsonResponse<ResponseData>(emptyResponseValid: true);
            if (success)
                Response = data;
        }
    }
}
