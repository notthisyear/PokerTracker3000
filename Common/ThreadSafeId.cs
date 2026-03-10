using System.Threading;

namespace PokerTracker3000.Common
{
    public static class ThreadSafeId
    {
        private static readonly Lock s_lock = new();
        private static int s_ctr = 0;

        public static int GetNext()
        {
            int id;
            lock (s_lock)
                id = s_ctr++;
            return id;
        }
    }
}
