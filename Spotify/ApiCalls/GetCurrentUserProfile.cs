using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokerTracker3000.Spotify.ApiCalls
{
    internal sealed class GetCurrentUserProfile : SpotifyApiCallBase
    {
        public record ResponseData
        {
            public readonly struct ExplicitContentStruct
            {
                /// <summary>
                /// When true, indicates that explicit content should not be played.
                /// </summary>
                public bool FilterEnabled { get; init; }

                /// <summary>
                /// When true, indicates that the explicit content setting is locked and can't be changed by the user.
                /// </summary>
                public bool FilterLocked { get; init; }
            }

            public readonly struct ExternalUrlsStruct
            {
                /// <summary>
                /// The Spotify URL for the object.
                /// </summary>
                public string? Spotify { get; init; }
            }

            public readonly struct FollowersStruct
            {
                /// <summary>
                /// This will always be set to null, as the Web API does not support it at the moment.
                /// </summary>
                public string? Href { get; init; }

                /// <summary>
                /// The total number of followers.
                /// </summary>
                public int Total { get; init; }
            }

            public readonly struct ImagesStruct
            {
                /// <summary>
                /// The source URL of the image.
                /// </summary>
                public string? Url { get; init; }

                /// <summary>
                /// The source URL of the image.
                /// </summary>
                public int? Height { get; init; }

                /// <summary>
                /// The image width in pixels.
                /// </summary>
                public int? Width { get; init; }
            }

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
            public ExplicitContentStruct ExplicitContent { get; init; }

            /// <summary>
            /// Known external URLs for this user.
            /// </summary>
            public ExternalUrlsStruct ExternalUrls { get; init; }

            /// <summary>
            /// Information about the followers of the user.
            /// </summary>
            public FollowersStruct Followers { get; init; }

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
            public List<ImagesStruct>? Images { get; init; }

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
            var (success, data) = await ReadAndDeserializeJsonResponse<ResponseData>();
            if (success)
                Response = data;
        }
    }
}
