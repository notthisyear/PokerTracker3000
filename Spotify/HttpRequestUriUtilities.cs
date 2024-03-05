using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PokerTracker3000.Spotify
{
    internal static partial class HttpRequestUriUtilities
    {
        [GeneratedRegex("(?<=((\\?)|(\\&)))((?<key>\\w+)\\=(?<value>[a-zA-Z0-9-_]+))", RegexOptions.Compiled)]
        private static partial Regex GetParseQueryRegexUri();

        private static readonly Regex s_parseQueryUri = GetParseQueryRegexUri();

        public static string GetQueryUri(string baseUri, Dictionary<string, string> parameters)
        {
            StringBuilder sb = new();
            sb.Append(baseUri);
            if (parameters.Count > 0)
            {
                sb.Append('?');
                sb.Append(string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}")));
            }
            return sb.ToString();
        }

        public static Dictionary<string, string> ParseQueryUri(string uri)
        {
            var result = new Dictionary<string, string>();
            if (!uri.Contains('?'))
                return result;

            var matches = s_parseQueryUri.Matches(uri);
            if (matches == default || matches.Count == 0)
                return result;

            foreach (var m in matches.Cast<Match>())
            {
                var key = string.Empty;
                var value = string.Empty;
                foreach (var g in m.Groups.Cast<Group>())
                {
                    if (g.Name == "key")
                        key = g.Value;
                    else if (g.Name == "value")
                        value = g.Value;
                }

                if (!string.IsNullOrEmpty(key) || !string.IsNullOrEmpty(value))
                    result.Add(key, value);
            }
            return result;
        }
    }
}
