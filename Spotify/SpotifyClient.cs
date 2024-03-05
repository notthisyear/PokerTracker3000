using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<bool> TryGetTokenForScopes(Authorizer.AuthorizationFlowType flowType, IProgress<string> progressReporter, int pkceVerifierLength = 0, params AccessScopeType[] scopes)
        {
            if (scopes.Length == 0)
                return false;

            List<AccessScopeType> scopesToInclude = new();
            if (_currentAccessToken != default)
            {
                var missingScopes = scopes.Where(x => !_currentAccessToken.HasScope(x));
                if (!missingScopes.Any())
                    return true;

                foreach (var existingScope in _currentAccessToken.Scopes!)
                    scopesToInclude.Add(existingScope);
            }

            foreach (var scope in scopes)
            {
                if (!scopesToInclude.Contains(scope))
                    scopesToInclude.Add(scope);
            }

            var token = flowType switch
            {
                Authorizer.AuthorizationFlowType.AuthorizationCodePkce =>
                    await Authorizer.TryAuthorizationUsingAuthorizationCodePkce(_listener, _client, scopesToInclude, _clientId, pkceVerifierLength, progressReporter),
                _ =>
                    throw new NotSupportedException($"Only AuthorizationFlowType '{Authorizer.AuthorizationFlowType.AuthorizationCodePkce}' is supported"),
            };

            if (token != default)
                _currentAccessToken = token;
            var call = new GetUserTopItems(GetUserTopItems.TopItemType.Artists);
            _ = await SendSpotifyApiCall(call);
            var call = new GetUserTopItems(GetUserTopItems.TopItemType.Artists);
            _ = await SendSpotifyApiCall(call);
            var call = new GetUserTopItems(GetUserTopItems.TopItemType.Artists);
            _ = await SendSpotifyApiCall(call);
            var call = new GetUserTopItems(GetUserTopItems.TopItemType.Artists);
            _ = await SendSpotifyApiCall(call);
            var call = new GetUserTopItems(GetUserTopItems.TopItemType.Artists);
            _ = await SendSpotifyApiCall(call);
            var call = new GetUserTopItems(GetUserTopItems.TopItemType.Artists);
            _ = await SendSpotifyApiCall(call);
            var call = new GetUserTopItems(GetUserTopItems.TopItemType.Artists);
            _ = await SendSpotifyApiCall(call);
            var call = new GetUserTopItems(GetUserTopItems.TopItemType.Artists);
            _ = await SendSpotifyApiCall(call);
            var call = new GetUserTopItems(GetUserTopItems.TopItemType.Artists);
            _ = await SendSpotifyApiCall(call);
            var call = new GetUserTopItems(GetUserTopItems.TopItemType.Artists);
            _ = await SendSpotifyApiCall(call);
            return token != default;
        }

        public async Task<bool> SendSpotifyApiCall(SpotifyApiCallBase call)
        {
            if (_currentAccessToken == default)
                return false;

            var tokenStatus = call.VerifyAccessToken(_currentAccessToken);
            if (tokenStatus == TokenStatus.TokenInsufficient)
                return false;

            if (tokenStatus == TokenStatus.TokenExpired)
            {
                var newToken = await Authorizer.TryRefreshToken(_client, _clientId, _currentAccessToken);
                if (newToken == default)
                    return false;
                _currentAccessToken = newToken;
            }

            var request = call.GetHttpRequestMessage(_currentAccessToken);  
            var response = await _client.SendHttpRequest(request);
            await call.SetHttpResponseMessage(response);
            return response != default;
        }
        #endregion
    }
}
