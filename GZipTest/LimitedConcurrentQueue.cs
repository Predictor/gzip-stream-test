using System.Threading;

namespace GZipTest
{
    public class LimitedConcurrentQueue<T>
    {
        private readonly ConcurrentQueue<T> internalQueue = new ConcurrentQueue<T>();
        private readonly SemaphoreSlim sem;

        public LimitedConcurrentQueue(int capacity)
        {
            sem = new SemaphoreSlim(capacity, capacity);
        }

        public int Count => internalQueue.Count;

        public void Enqueue(T item)
        {
            sem.Wait();
            internalQueue.Enqueue(item);
        }

        public T Dequeue()
        {
            var item = internalQueue.Dequeue();
            sem.Release();
            return item;
        }
    }
}