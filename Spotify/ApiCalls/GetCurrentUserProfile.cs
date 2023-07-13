using System.Collections.Generic;
using System.Threading.Tasks;
using PokerTracker3000.Spotify.ApiCalls.Shared;

namespace PokerTracker3000.Spotify.ApiCalls
{
    internal sealed class GetCurrentUserProfile : SpotifyApiCallBase
    {
        public record ResponseData
        {
            /// <summary>
            /// The country of the user, as set in the user's account profile. An ISO 3166-1 alpha-2 country code.
            /// <para>This field is only available when the current user has granted access to the user-read-private scope.</para>
            /// </summary>
            public string? Country { get; init; }

            /// <summary>
            /// The name displayed on the user's profile. null if not available.
            /// </summary>
            public string? DisplayName { get; init; }

            /// <summary>
            /// The user's email address, as entered by the user when creating their account.
            /// <para>Important! This email address is unverified; there is no proof that it actually belongs to the user.
            /// This field is only available when the current user has granted access to the user-read-email scope.</para>
            /// </summary>
            public string? Email { get; init; }

            /// <summary>
            /// The user's explicit content settings.
            /// <para>This field is only available when the current user has granted access to the user-read-private scope.</para>
            /// </summary>
            public ExplicitContent ExplicitContent { get; init; }

            /// <summary>
            /// Known external URLs for this user.
            /// </summary>
            public ExternalUrlsObject ExternalUrls { get; init; }

            /// <summary>
            /// Information about the followers of the user.
            /// </summary>
            public FollowersObject Followers { get; init; }

            /// <summary>
            /// A link to the Web API endpoint for this user.
            /// </summary>
            public string? Href { get; init; }

            /// <summary>
            /// The Spotify user ID for the user.
            /// </summary>
            public string? Id { get; init; }

            /// <summary>
            /// The user's profile image.
            /// </summary>
            public List<ImageObject>? Images { get; init; }

            /// <summary>
            /// The user's Spotify subscription level: "premium", "free", etc. (The subscription level "open" can be considered the same as "free".)
            /// <para>This field is only available when the current user has granted access to the user-read-private scope.</para>
            /// </summary>
            public string? Product { get; init; }

            /// <summary>
            /// The object type: "user"
            /// </summary>
            public string? Type { get; init; }


            /// <summary>
            /// The Spotify URI for the user.
            /// </summary>
            public string? Uri { get; init; }
        }

        public ResponseData? Response { get; private set; }

        public override SpotifyApiCallType ApiCall => SpotifyApiCallType.GetCurrentUserProfile;

        protected override SpotifyHttpClient.HttpRequestMethod RequestMethod => SpotifyHttpClient.HttpRequestMethod.Get;

        protected override string Endpoint => SpotifyEndpoint.UserBaseEndpoint;

        protected override List<AccessScopeType> Scopes => new() { AccessScopeType.UserReadPrivate, AccessScopeType.UserReadEmail };

        protected override async Task ParseResponse()
        {
            var (success, _, data) = await ReadAndDeserializeJsonResponse<ResponseData>();
            if (success)
                Response = data;
        }
    }
}
