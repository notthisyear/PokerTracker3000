using System;
using System.Collections.Generic;
using PokerTracker3000.Spotify;

namespace PokerTracker3000.Common
{
    internal static class AccessScopeExtensionMethods
    {
        private static readonly Dictionary<string, AccessScopeType> s_accessScopeMap = new();

        public static bool TryGetAccessScopeTypeFromString(this string rawScopeType, out AccessScopeType scopeType)
        {
            lock (s_accessScopeMap)
            {
                if (s_accessScopeMap.Count == 0)
                {
                    foreach (AccessScopeType scope in Enum.GetValues(typeof(AccessScopeType)))
                        s_accessScopeMap.Add(scope.GetCustomAttributeFromEnum<AccessScopeAttribute>().attr!.ScopeName, scope);
                }
            }

            return s_accessScopeMap.TryGetValue(rawScopeType, out scopeType);
        }
    }
}
