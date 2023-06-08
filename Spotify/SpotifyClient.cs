using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PokerTracker3000.Spotify.ApiCalls;

namespace PokerTracker3000.Spotify
{
    internal class SpotifyClient
    {
        #region Private fields
        private readonly string _clientId;
        private readonly SpotifyHttpClient _client;
        private readonly SpotifyHttpListener _listener;
        private SpotifyAccessToken? _currentAccessToken;
        #endregion

        public SpotifyClient(string clientId, int localListenerPort)
        {
            _clientId = clientId;

            // From https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines
            var client = new HttpClient(new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(15)
            });
            var listener = new HttpListener();

            _client = new(client);
            _listener = new(listener, localListenerPort);
        }

        #region Public methods
        public async Task<bool> TryAuthorize(Authorizer.AuthorizationFlowType flowType, IProgress<string> progressReporter, int pkceVerifierLength = 0)
        { 
            var token = flowType switch
            {
                Authorizer.AuthorizationFlowType.AuthorizationCodePkce => await Authorizer.TryAuthorizationUsingAuthorizationCodePkce(_listener, _client, _clientId, pkceVerifierLength, progressReporter),
                _ => throw new NotSupportedException($"Only AuthorizationFlowType '{Authorizer.AuthorizationFlowType.AuthorizationCodePkce}' is supported"),
            };

            if (token != default)
                _currentAccessToken = token;

            var call = new GetUserTopItems(GetUserTopItems.TopItemType.Artists);
            _ = await SendSpotifyApiCall(call);
            return token != default;
        }

        public async Task<bool> SendSpotifyApiCall(SpotifyApiCallBase call)
        {
            if (_currentAccessToken == default)
                return false;

            if (call.VerifyAccessToken(_currentAccessToken) != TokenStatus.TokenOk)
            {
                // TODO: Deal with Token.Expired
                return false;
            }

            var request = call.GetHttpRequestMessage(_currentAccessToken);
            var response = await _client.SendHttpRequest(request);
            await call.SetHttpResponseMessage(response);
            return response != default;
        }
        #endregion
    }
}
