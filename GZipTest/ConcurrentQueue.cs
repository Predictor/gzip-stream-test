using System.Collections.Generic;
using System.Threading;

namespace GZipTest
{
    public class ConcurrentQueue<T>
    {
        private readonly Queue<T> internalQueue;
        private readonly SemaphoreSlim sem = new SemaphoreSlim(1, 1);

        public ConcurrentQueue()
        {
            internalQueue = new Queue<T>();
        }

        public ConcurrentQueue(IEnumerable<T> source)
        {
            internalQueue = new Queue<T>(source);
        }

        public int Count => internalQueue.Count;

        public void Enqueue(T item)
        {
            sem.Wait();
            try
            {
                internalQueue.Enqueue(item);
            }
            finally
            {
                sem.Release();
            }
        }

        public T Dequeue()
        {
            sem.Wait();
            try
            {
                return internalQueue.Dequeue();
            }
            finally
            {
                sem.Release();
            }
        }
    }
}
