using System.Collections.Generic;
using System.Threading.Tasks;
using PokerTracker3000.Common;
using PokerTracker3000.Spotify.ApiCalls.Shared;

namespace PokerTracker3000.Spotify.ApiCalls
{
    internal sealed class GetPlaybackState : SpotifyApiCallBase
    {
        public record ResponseData
        {
            /// <summary>
            /// The device that is currently active.
            /// </summary>
            public DeviceObject Device { get; init; }

            /// <summary>
            /// Either off, track or context.
            /// </summary>
            public string? RepeatState { get; init; }

            /// <summary>
            /// If shuffle is on or off.
            /// </summary>
            public bool ShuffleState { get; init; }

            /// <summary>
            /// A Context Object. Can be null.
            /// </summary>
            public ContextObject? Context { get; init; }

            /// <summary>
            /// Unix Millisecond Timestamp when data was fetched.
            /// </summary>
            public long Timestamp { get; init; }

            /// <summary>
            /// Progress into the currently playing track or episode. Can be null.
            /// </summary>
            public int? ProgressMs { get; init; }

            /// <summary>
            /// If something is currently playing, return true.
            /// </summary>
            public bool IsPlaying { get; init; }

            /// <summary>
            /// The currently playing track or episode. Can be null.
            /// </summary>
            public OneOf<TrackObject, EpisodeObject>? TrackOrEpisode { get; set; }

            /// <summary>
            /// The object type of the currently playing item. Can be one of track, episode, ad or unknown
            /// </summary>
            public string? CurrentlyPlayingType { get; init; }

            /// <summary>
            ///  Allows to update the user interface based on which playback actions are available within the current context.
            /// </summary>
            public ActionsObject Actions { get; init; }
        }

        public ResponseData? Response { get; private set; }

        public override SpotifyApiCallType ApiCall => SpotifyApiCallType.GetPlaybackState;

        protected override SpotifyHttpClient.HttpRequestMethod RequestMethod => SpotifyHttpClient.HttpRequestMethod.Get;

        protected override List<AccessScopeType> Scopes => new() { AccessScopeType.UserReadPlaybackState };

        protected override string Endpoint => SpotifyEndpoint.PlayerBaseEndpoint;

        protected override async Task ParseResponse()
        {
            var (success, isEmpty, data) = await ReadAndDeserializeJsonResponse<ResponseData>(true, (r, s) =>
            {
                return default;
            });
            //    var itemContent = s.TryGetTokenFromRawInput("item");
            //    if (itemContent == default)
            //        return default;
            //    var serializer = JsonExtensionMethods.GetSnakeCaseNameingSerializer();

            //    TrackObject? trackItem = default;
            //    trackItem = itemContent.ToObject<TrackObject>(serializer);
            //    if (trackItem == default)
            //    {
            //        var episodeItem = itemContent.ToObject<EpisodeObject>(serializer);
            //        if (episodeItem != default)
            //            r.TrackOrEpisode = new() { Second = episodeItem! };
            //    }
            //    else
            //    {
            //        r.TrackOrEpisode = new() { First = trackItem };
            //    }
            //    return default;

            if (success)
                Response = data;
        }
    }
}
