using System;
using PokerTracker3000.Common;

namespace PokerTracker3000.Spotify
{
    internal enum AccessScopeType
    {
        [AccessScope("streaming", "Control playback of a Spotify track. This scope is currently available to the Web Playback SDK. The user must have a Spotify Premium account.", "Play content and control playback on your other devices.")]
        Streaming,

        [AccessScope("user-read-email", "Read access to user’s email address.", "Get your real email address.")]
        UserReadEmail,

        [AccessScope("user-read-private", "Read access to user’s subscription details (type of user account).", "Access your subscription details.")]
        UserReadPrivate,

        [AccessScope("user-read-playback-state", "Read access to a user’s player state.", "Read your currently playing content and Spotify Connect devices information.")]
        UserReadPlaybackState,

        [AccessScope("user-modify-playback-state", "Write access to a user’s playback state", "Control playback on your Spotify clients and Spotify Connect devices.")]
        UserModifyPlaybackState,

        [AccessScope("user-read-currently-playing", "Read access to a user’s currently playing content.", "Read your currently playing content.")]
        UserReadCurrentlyPlaying
    }

    [AttributeUsage(AttributeTargets.Field)]
    internal class AccessScopeAttribute : Attribute
    {
        public string Description { get; }

        public string ShownToUser { get; }

        public string ScopeName { get; }

        public AccessScopeAttribute(string scopeName, string description, string shownToUser = "")
        {
            ScopeName = scopeName;
            Description = description;
            ShownToUser = shownToUser;
        }
    }

    internal static class ScopeUtilities
    {
        public static string ScopeName(this AccessScopeType type)
            => type.GetCustomAttributeFromEnum<AccessScopeAttribute>().attr!.ScopeName;
    }
}
