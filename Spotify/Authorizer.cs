using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PokerTracker3000.Common;

namespace PokerTracker3000.Spotify
{
    internal static class Authorizer
    {
        public enum AuthorizationFlowType
        {
            AuthorizationCode,
            AuthorizationCodePkce, // Proof Key for Code Exchange (PKCE)
            ClientCredentials,
            ImplicitGrant
        };

        #region Private fields
        private static readonly char[] s_randomCharacterSeed =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_".ToCharArray();
        private static string? s_currentPkceState;
        private static TaskCompletionSource<(bool success, string codeOrError)>? s_pkceAuthenticationCodeAwaiter;
        private static Timer? s_pkceAuthenticationCodeTimeoutTimer;
        #endregion

        public static async Task<SpotifyAccessToken?> TryAuthorizationUsingAuthorizationCodePkce(SpotifyHttpListener listener, SpotifyHttpClient client, List<AccessScopeType> scopes, string clientId, int pkceVerifierLength, IProgress<string> progressReporter)
        {
            if (pkceVerifierLength < 43 || pkceVerifierLength > 128)
                throw new ArgumentOutOfRangeException($"{nameof(pkceVerifierLength)}", "Parameter must be within the range 43 - 128");
            var (codeVerifier, codeChallenge) = GetCodeVerifierAndChallenge(pkceVerifierLength);

            var queryUri = GetPkceGetQuery(listener.ListeningOnUrl, scopes, clientId, codeChallenge);

            listener.RegisterCallbackForNextRequest(HandlePkceAuthorizationGetQuery);
            s_pkceAuthenticationCodeAwaiter = new(TaskCreationOptions.RunContinuationsAsynchronously);
            progressReporter.Report("Waiting for user to complete authentication process...");

            s_pkceAuthenticationCodeTimeoutTimer = new((s) =>
            {
                if (s_pkceAuthenticationCodeAwaiter != default)
                    s_pkceAuthenticationCodeAwaiter.SetResult((false, "Request timed out"));
            }, default, 60000, Timeout.Infinite);

            var p = Process.Start(new ProcessStartInfo(queryUri) { UseShellExecute = true });
            var (success, codeOrError) = await s_pkceAuthenticationCodeAwaiter.Task.ConfigureAwait(false);

            s_pkceAuthenticationCodeAwaiter = default;
            s_pkceAuthenticationCodeTimeoutTimer.Change(Timeout.Infinite, Timeout.Infinite);
            s_pkceAuthenticationCodeTimeoutTimer.Dispose();
            s_pkceAuthenticationCodeTimeoutTimer = default;

            p!.Kill();

            if (!success)
            {
                progressReporter.Report(codeOrError);
                return default;
            }

            progressReporter.Report("Getting access token...");

            var timestamp = DateTime.UtcNow;
            var (requestSuccessful, statusCode, reason, token) = await client.SendRequestAndDeserializeResponse<SpotifyAccessToken>(SpotifyHttpClient.HttpRequestMethod.Post,
                SpotifyEndpoint.OAuthTokenEndpoint,
                GetPkceFetchContent(codeOrError, listener.ListeningOnUrl, clientId, codeVerifier));

            if (!requestSuccessful)
            {
                progressReporter.Report($"Token HTTP request failed ({statusCode}) - '{reason}'");
            }
            else
            {
                if (token != default)
                {
                    token.SetExpiration(timestamp);
                    token.ParseScopes();
                }
                progressReporter.Report(token == default ? $"Token parsing failed - '{reason}'" : "Successfully got token!");
            }

            return token;
        }

        public static async Task<SpotifyAccessToken?> TryRefreshToken(SpotifyHttpClient client, string clientId, SpotifyAccessToken token)
        {
            var timestamp = DateTime.UtcNow;
            var (requestSuccessful, _, _, refreshedToken) = await client.SendRequestAndDeserializeResponse<SpotifyAccessToken>(
                SpotifyHttpClient.HttpRequestMethod.Post,
                SpotifyEndpoint.OAuthTokenEndpoint,
                GetPkceTokenRefreshContentContent(clientId, token.RefreshToken!));

            if (requestSuccessful)
            {
                if (refreshedToken != default)
                {
                    refreshedToken.SetExpiration(timestamp);
                    refreshedToken.ParseScopes();
                }
            }

            return refreshedToken;
        }

        #region Flow for AuthorizationCodePkce method
        private static (string codeVerifier, string codeChallenge) GetCodeVerifierAndChallenge(int pkceVerifierLength)
        {
            var codeVerifier = GetRandomString(pkceVerifierLength);
            var codeChallenge = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier)));

            if (codeChallenge.Contains('+') || codeChallenge.Contains('/') || codeChallenge.Contains('='))
                codeChallenge = codeChallenge.Replace('+', '-').Replace('/', '_').TrimEnd('=');

            return (codeVerifier, codeChallenge);
        }

        private static string GetPkceGetQuery(string listeningOnUrl, List<AccessScopeType> scopes, string clientId, string codeChallenge)
        {
            s_currentPkceState = GetRandomString(16);
            var parameters = new Dictionary<string, string>
            {
                { "response_type", "code" },
                { "client_id", clientId },
                { "scope", $"{AccessScopeType.UserReadPrivate.GetCustomAttributeFromEnum<AccessScopeAttribute>().attr!.ScopeName} {AccessScopeType.UserReadEmail.GetCustomAttributeFromEnum<AccessScopeAttribute>().attr!.ScopeName}" },
                { "state", s_currentPkceState },
                { "redirect_uri", listeningOnUrl.TrimEnd('/') },
                { "code_challenge_method", "S256" },
                { "code_challenge", codeChallenge },
            };
            return HttpRequestUriUtilities.GetQueryUri(SpotifyEndpoint.AuthorizeEndpoint, parameters);
        }

        private static HttpContent GetPkceFetchContent(string code, string listeningOnUrl, string clientId, string codeVerifier)
            => new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", listeningOnUrl.TrimEnd('/') },
                { "client_id", clientId },
                { "code_verifier", codeVerifier },
            });

        private static void HandlePkceAuthorizationGetQuery((HttpListenerRequest request, HttpListenerResponse response) data)
        {
            var success = false;
            var codeOrError = string.Empty;

            if (s_pkceAuthenticationCodeAwaiter == null || string.IsNullOrEmpty(s_currentPkceState))
                return;

            if (data.request.Url == null)
                codeOrError = "Request URL not set";

            if (string.IsNullOrEmpty(codeOrError))
            {
                var queryParameters = HttpRequestUriUtilities.ParseQueryUri(data.request.Url!.AbsoluteUri);
                if (queryParameters.TryGetValue("state", out var state))
                {
                    if (!state.Equals(s_currentPkceState, StringComparison.Ordinal))
                        codeOrError = "Request state mismatch";
                }

                if (string.IsNullOrEmpty(codeOrError) && queryParameters.TryGetValue("error", out var error))
                    codeOrError = $"Error - '{error}'";

                if (string.IsNullOrEmpty(codeOrError))
                {
                    success = queryParameters.TryGetValue("code", out var code);
                    codeOrError = success ? code : "Request URL missing 'code' parameter";
                }
            }

            data.response.AddHeader("Content-Type", "text/html");
            var content = Encoding.UTF8.GetBytes(GetHtmlResponseForPkceGetQuery(success, codeOrError));
            data.response.ContentLength64 = content.Length;
            var responseStream = data.response.OutputStream;
            responseStream.Write(content, 0, content.Length);
            responseStream.Close();
            data.response.Close();

            s_pkceAuthenticationCodeAwaiter.SetResult((success, codeOrError!));
        }

        private static string GetHtmlResponseForPkceGetQuery(bool success, string? codeOrError)
            => $"<html><body>{(success ? "Success" : codeOrError)}. You can close this window.</body></html>";

        private static HttpContent GetPkceTokenRefreshContentContent(string clientId, string refreshToken)
            => new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken },
                { "client_id", clientId }
            });
        #endregion
        private static string GetRandomString(int length)
        {
            var data = new byte[4 * length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(data);
            var sb = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                sb.Append(s_randomCharacterSeed[rnd % s_randomCharacterSeed.Length]);
            }

            return sb.ToString();
        }
    }
}
