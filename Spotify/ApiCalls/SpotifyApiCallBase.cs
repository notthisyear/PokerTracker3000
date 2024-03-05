using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PokerTracker3000.Common;
using static PokerTracker3000.Spotify.SpotifyHttpClient;

namespace PokerTracker3000.Spotify.ApiCalls
{
    public enum SpotifyApiCallType
    {
        GetCurrentUserProfile,
        GetAvailableDevices,
        GetPlaybackState,
        StartOrResumePlayback,
        PausePlayback
    }

    internal abstract class SpotifyApiCallBase
    {
        #region Public properties
        public abstract SpotifyApiCallType ApiCall { get; }

        public bool SuccessfulStatusCode { get; private set; }

        public HttpStatusCode ReturnStatusCode { get; private set; }

        public string? ErrorReasonString { get; private set; }
        #endregion

        #region Protected properties
        protected abstract HttpRequestMethod RequestMethod { get; }

        protected abstract List<AccessScopeType> Scopes { get; }

        protected abstract string Endpoint { get; }

        protected HttpResponseMessage? ResponseMessage { get; private set; }

        protected delegate Exception? CustomResponseHandler<T>(T response, string rawContent);
        #endregion

        #region Private constants
        private const string AuthorizationHeaderKey = "Authorization";

        #endregion

        #region Public methods
        public TokenStatus VerifyAccessToken(SpotifyAccessToken accessToken)
        {
            foreach (var requiredScope in Scopes)
            {
                if (!accessToken.HasScope(requiredScope))
                    return TokenStatus.TokenInsufficient;
            }

            return accessToken.HasExpired() ? TokenStatus.TokenExpired : TokenStatus.TokenOk;
        }

        public HttpRequestMessage GetHttpRequestMessage(SpotifyAccessToken accessToken)
        {
            var request = new HttpRequestMessage(RequestMethod switch
            {
                HttpRequestMethod.Post => HttpMethod.Post,
                HttpRequestMethod.Put => HttpMethod.Put,
                HttpRequestMethod.Get => HttpMethod.Get,
                _ => throw new NotImplementedException(),
            }, GetEndpoint());

            request.Headers.Add(AuthorizationHeaderKey, GetAuthorizationHeaderValue(accessToken.TokenType!, accessToken.AccessToken!));
            AddBodyToRequestIfNeeded(request);
            return request;
        }

        public async Task SetHttpResponseMessage(HttpResponseMessage? response)
        {
            ResponseMessage = response;
            SuccessfulStatusCode = ResponseMessage != default && ResponseMessage.IsSuccessStatusCode;
            ReturnStatusCode = ResponseMessage?.StatusCode ?? HttpStatusCode.Unused;
            ErrorReasonString = ResponseMessage?.ReasonPhrase ?? "No response received";
            await ParseResponse();
        }
        #endregion

        #region Protected methods
        protected virtual void AddBodyToRequestIfNeeded(HttpRequestMessage request) { }

        protected virtual Task ParseResponse() => Task.CompletedTask;

        protected virtual string GetEndpoint() => Endpoint;

        protected async Task<(bool success, bool isEmpty, T? response)> ReadAndDeserializeJsonResponse<T>(bool emptyResponseValid = false, CustomResponseHandler<T>? customResponseHandler = default)
        {
            if (SuccessfulStatusCode && (ResponseMessage?.IsSuccessStatusCode ?? false))
            {
                var content = await ResponseMessage.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(content))
                    return (emptyResponseValid, true, default);

                var (r, e) = content.DeserializeJsonString<T>(convertSnakeCaseToPascalCase: true);
                if (customResponseHandler != default)
                    e = customResponseHandler.Invoke(r!, content);
                return (e == default, false, r);
            }

            return (false, true, default);
        }
        #endregion

        private static string GetAuthorizationHeaderValue(string tokenType, string accessToken)
            => tokenType + " " + accessToken;
    }
}
