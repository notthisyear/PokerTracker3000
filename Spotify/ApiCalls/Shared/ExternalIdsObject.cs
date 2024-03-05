namespace PokerTracker3000.Spotify.ApiCalls.Shared
{
    public readonly struct ExternalIdsObject
    {
        /// <summary>
        /// <a href="http://en.wikipedia.org/wiki/International_Standard_Recording_Code">International Standard Recording Code</a>
        /// </summary>
        public string? Isrc { get; init; }

        /// <summary>
        /// <a href="http://en.wikipedia.org/wiki/International_Article_Number_%28EAN%29">International Article Number</a>
        /// </summary>
        public string? Ean { get; init; }

        /// <summary>
        /// <a href="http://en.wikipedia.org/wiki/Universal_Product_Code">Universal Product Code</a>
        /// </summary>
        public string? Upc { get; init; }
    }
}