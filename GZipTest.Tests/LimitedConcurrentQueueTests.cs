using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GZipTest.Tests
{
    [TestClass]
    public class LimitedConcurrentQueueTests
    {
        [TestMethod]
        public void RespectsLimit()
        {
            var entries = Enumerable.Range(0, 1000).ToList();
            var queue = new LimitedConcurrentQueue<int>(500);
            var threads = new List<Thread>();
            foreach (var i in entries)
            {
                var t = new Thread(() => queue.Enqueue(i));
                threads.Add(t);
                t.Start();
            }

            Thread.Sleep(100);
            Assert.AreEqual(500, queue.Count);

            for (int i = 0; i < 500; i++)
            {
                queue.Dequeue();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
            Assert.AreEqual(500, queue.Count);
        }
    }
}