using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PokerTracker3000.Common;

namespace PokerTracker3000.Spotify
{
    internal class SpotifyHttpClient
    {
        public enum HttpRequestMethod
        {
            Post,
            Put,
            Get
        }

        private readonly HttpClient _client;

        public SpotifyHttpClient(HttpClient client)
        {
            _client = client;
            _client.Timeout = TimeSpan.FromSeconds(5);
        }

        public async Task<HttpResponseMessage> SendHttpRequest(HttpRequestMessage request)
            => await _client.SendAsync(request);

        public async Task<(bool requestSuccessful, HttpStatusCode statusCode, string reason, T? deserializedResponse)> SendRequestAndDeserializeResponse<T>(HttpRequestMethod method, string requestUri, HttpContent? content = default)
        {
            var response = await SendRequest(method, requestUri, content);
            if (!response.IsSuccessStatusCode)
                return (false, response.StatusCode, response.ReasonPhrase ?? string.Empty, default);

            var mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
            if (string.IsNullOrEmpty(mediaType) || !mediaType.Equals("application/json", StringComparison.Ordinal))
                return (true, response.StatusCode, $"Unexpected response type '{response.Content.Headers.ContentType?.MediaType ?? "null"}'", default);

            var responseContent = await response.Content.ReadAsStringAsync();
            var (r, e) = responseContent.DeserializeJsonString<T>(convertSnakeCaseToPascalCase: true);
            return (true, response.StatusCode, e == default ? string.Empty : $"Could not deserialize response - {e.GetType()}: {e.Message}", r);
        }

        public async Task<HttpResponseMessage> SendRequest(HttpRequestMethod method, string requestUri, HttpContent? content = default)
        {
            if ((method == HttpRequestMethod.Post || method == HttpRequestMethod.Put) && content == null)
                throw new ArgumentNullException($"Argument {nameof(content)} cannot be null if HttpRequestMethod is either {HttpRequestMethod.Post} or {HttpRequestMethod.Put}");

            return method switch
            {
                HttpRequestMethod.Post => await _client.PostAsync(requestUri, content),
                HttpRequestMethod.Put => await _client.PutAsync(requestUri, content),
                HttpRequestMethod.Get => await _client.GetAsync(requestUri),
                _ => throw new NotImplementedException($"HttpRequestMethod '{method}' is not implemented"),
            };
        }
    }
}
