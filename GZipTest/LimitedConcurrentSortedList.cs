using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GZipTest
{
    public class LimitedConcurrentSortedList<TKey, TValue>
    {
        private readonly SemaphoreSlim syncSem = new SemaphoreSlim(1, 1);
        private readonly SortedList<TKey, TValue> internalList = new SortedList<TKey, TValue>();
        private readonly SemaphoreSlim limitSem;

        public LimitedConcurrentSortedList(int capacity)
        {
            limitSem = new SemaphoreSlim(capacity, capacity);
        }

        public int Count => internalList.Count;

        public void Add(TKey key, TValue value)
        {
            limitSem.Wait();
            syncSem.Wait();
            try
            {
                internalList.Add(key, value);
            }
            finally
            {
                syncSem.Release();
            }
        }

        public TValue GetNextAndRemove()
        {
            syncSem.Wait();

            try
            {
                if (internalList.Count == 0)
                {
                    return default;
                }
                var item = internalList.First();
                internalList.Remove(item.Key);
                limitSem.Release();
                return item.Value;
            }
            finally
            {
                syncSem.Release();
            }
        }
    }
}