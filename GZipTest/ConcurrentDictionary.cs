using System.Collections.Generic;
using System.Threading;

namespace GZipTest
{
    public class ConcurrentDictionary<TKey, TValue>
    {
        private readonly SemaphoreSlim syncSem = new SemaphoreSlim(1, 1);
        private readonly Dictionary<TKey, TValue> internalDict = new Dictionary<TKey, TValue>();

        public ConcurrentDictionary()
        {
        }

        public int Count => internalDict.Count;

        public void Add(TKey key, TValue value)
        {
            syncSem.Wait();
            try
            {
                internalDict.Add(key, value);
            }
            finally
            {
                syncSem.Release();
            }
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            syncSem.Wait();

            try
            {
                if (internalDict.TryGetValue(key, out value))
                {
                    internalDict.Remove(key);
                    return true;
                }

                return false;
            }
            finally
            {
                syncSem.Release();
            }
        }
    }
}