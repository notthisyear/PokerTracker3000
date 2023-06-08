using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PokerTracker3000.Spotify.ApiCalls
{
    internal sealed class GetUserTopItems : SpotifyApiCallBase
    {
        public enum TopItemType
        {
            Artists,
            Tracks
        }

        public enum TimeRangeType
        {
            ShortTerm,
            MediumTerm,
            LongTerm
        }

        public override SpotifyApiCallType ApiCall => SpotifyApiCallType.GetUserTopItems;

        protected override SpotifyHttpClient.HttpRequestMethod RequestMethod => SpotifyHttpClient.HttpRequestMethod.Get;

        protected override List<AccessScopeType> Scopes => new() { AccessScopeType.UserTopRead };

        protected override string Endpoint => SpotifyEndpoint.UserBaseEndpoint + "/top/";

        private readonly string _topItemType;
        private readonly string _timeRangeType;
        private readonly int _limit;
        private readonly int _offset;

        public GetUserTopItems(TopItemType itemType, TimeRangeType timeRangeType = TimeRangeType.MediumTerm, int limit = 20, int offset = 0)
        {
            _topItemType = itemType switch
            {
                TopItemType.Artists => "artists",
                TopItemType.Tracks => "tracks",
                _ => throw new NotImplementedException()
            };

            _timeRangeType = timeRangeType switch {
                TimeRangeType.ShortTerm => "short_term",
                TimeRangeType.MediumTerm => "medium_term",
                TimeRangeType.LongTerm => "long_term",
                _ => throw new NotImplementedException()
            };

            _limit = limit;
            _offset = offset;
        }

        // TODO: Add arguments to body
        protected override void AddBodyToRequestIfNeeded(HttpRequestMessage request) => base.AddBodyToRequestIfNeeded(request);

        // TODO: Parse response
        protected override Task ParseResponse() => base.ParseResponse();

        protected override string GetEndpoint() => Endpoint + _topItemType;
    }
}
