using System;

namespace PokerTracker3000.Spotify
{
    internal enum AccessScopeType
    {
        [AccessScope("user-read-email", "Read access to user’s email address.", "Get your real email address.")]
        UserReadEmail,

        [AccessScope("user-read-private", "Read access to user’s subscription details (type of user account).", "Access your subscription details.")]
        UserReadPrivate,

        [AccessScope("user-top-read", "Read access to a user's top artists and tracks.", "Read your top artists and content.")]
        UserTopRead
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
}
