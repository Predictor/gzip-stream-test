using System;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GZipTest.Tests
{
    [TestClass]
    public class MemoryLimiterTests
    {
        [TestMethod]
        public void PushToTheLimit()
        {
            var queue = new ConcurrentQueue<object>();
            var executor = new ParallelWorkExecutor(() =>
            {
                MemoryLimiter.Wait();
                queue.Enqueue(Enumerable.Range(0, 1000).Select(i=>Guid.NewGuid()).ToArray());
                Thread.Sleep(1);
            }, () => false, Environment.ProcessorCount);

            executor.Start();

            while (MemoryLimiter.IsEnoughMemory()) Thread.Sleep(1);
            Thread.Sleep(1000);
            var count = queue.Count;
            executor.Cancell(false);
            Assert.AreEqual(count, queue.Count, Environment.ProcessorCount * 2);
            while (queue.Count > 0)
            {
                queue.Dequeue();
            }
            Assert.IsTrue(MemoryLimiter.IsEnoughMemory());
        }
    }
}
