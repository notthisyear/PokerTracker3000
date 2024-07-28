using System.Collections.Generic;
using System.Windows.Media;

namespace PokerTracker3000.WpfComponents
{
    public enum IconType
    {
        None,
        Play,
        Pause,
        Spotify
    }

    public static class IconDataProvider
    {
        private const string PlayIconPathData = "m 27 18.8 l -19.2474 10.545 c -2.8342 1.5947 -6.3344 -0.4551 -6.3344 -3.7037 v -21.0863 c 0 -3.2523 3.5002 -5.2984 6.3344 -3.7037 l 19.2474 10.545 c 2.8897 1.6243 2.8897 5.7831 0 7.4074 z";
        private const string PauseIconPathData = "m 8.125 0 a 4.375 4.375 90 0 0 -4.375 4.375 v 21.25 a 4.375 4.375 90 0 0 8.75 0 v -21.25 a 4.375 4.375 90 0 0 -4.375 -4.375 z m 13.75 0 a 4.375 4.375 90 0 0 -4.375 4.375 v 21.25 a 4.375 4.375 90 0 0 8.75 0 v -21.25 a 4.375 4.375 90 0 0 -4.375 -4.375 z";
        private const string SpotifyIconPathData = "m 16 0 c -8.85 0 -16 7.1685 -16 16 c 0 8.85 7.1685 16 16 16 c 8.85 0 16 -7.1685 16 -16 s -7.1685 -16 -16 -16 z m 7.34 23.092 c -0.2865 0.478 -0.8985 0.6115 -1.3765 0.325 c -3.766 -2.294 -8.4875 -2.81 -14.07 -1.5485 c -0.535 0.115 -1.07 -0.21 -1.185 -0.745 s 0.21 -1.07 0.745 -1.185 c 6.098 -1.395 11.335 -0.803 15.541 1.778 a 1 1 90 0 1 0.344 1.3765 z m 1.95 -4.3585 c -0.363 0.5925 -1.128 0.765 -1.72 0.42 c -4.301 -2.638 -10.858 -3.4025 -15.9425 -1.8735 c -0.669 0.191 -1.357 -0.172 -1.5485 -0.822 c -0.191 -0.669 0.172 -1.357 0.841 -1.5485 c 5.811 -1.7585 13.037 -0.9175 17.988 2.122 c 0.5735 0.344 0.745 1.1085 0.3825 1.7015 z m 0.172 -4.55 c -5.1615 -3.0585 -13.668 -3.345 -18.6 -1.854 c -0.784 0.2485 -1.625 -0.21 -1.8735 -0.994 s 0.21 -1.625 0.994 -1.8735 c 5.6585 -1.72 15.0635 -1.3765 20.99 2.141 c 0.7075 0.42 0.9365 1.338 0.516 2.045 c -0.3825 0.7265 -1.319 0.956 -2.0265 0.535 z";

        private static readonly Dictionary<IconType, Geometry> s_iconGeometryMap = new()
        {
            { IconType.None, Geometry.Parse(string.Empty) },
            { IconType.Play, Geometry.Parse(PlayIconPathData) },
            { IconType.Pause, Geometry.Parse(PauseIconPathData) },
            { IconType.Spotify, Geometry.Parse(SpotifyIconPathData) },
        };

        public static Geometry GetDataForIcon(IconType type)
        {
            if (s_iconGeometryMap.TryGetValue(type, out var geometry))
                return geometry;
            return s_iconGeometryMap[IconType.None];
        }
    }
}
