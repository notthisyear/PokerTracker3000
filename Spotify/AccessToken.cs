using System;
using System.Collections.Generic;
using PokerTracker3000.Common;

namespace PokerTracker3000.Spotify
{
    public enum TokenStatus
    {
        TokenOk,
        TokenInsufficient,
        TokenExpired
    }

    internal record SpotifyAccessToken
    {
        public string? AccessToken { get; init; }

        public string? TokenType { get; init; }

        public List<AccessScopeType>? Scopes { get; private set; }

        public string? Scope { get; init; }

        public int ExpiresIn { get; init; }

        public string? RefreshToken { get; init; }

        private DateTime _expirationTime = DateTime.MinValue;
        public bool HasScope(AccessScopeType scope)
            => Scopes!.Contains(scope);

        public void ParseScopes()
        {
            if (string.IsNullOrEmpty(Scope))
                return;

            Scopes = new();
            var rawScopes = Scope.Split(' ');

            foreach (var rawScope in rawScopes)
            {
                if (rawScope.TryGetAccessScopeTypeFromString(out var scopeType))
                    Scopes.Add(scopeType);
                else
                    throw new NotSupportedException($"Unknown access scope '{rawScope}'");
            }
        }

        public bool HasExpired()
            => _expirationTime < DateTime.UtcNow;

        public void SetExpiration(DateTime timestamp)
        {
            _expirationTime = timestamp.AddSeconds(ExpiresIn);
        }
    }
}
