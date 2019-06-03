using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GZipTest.Tests
{
    [TestClass]
    public class ConcurrentQueueTests
    {
        [TestMethod]
        public void ConcurrentEnqueueAndDequeue()
        {
            var entries = Enumerable.Range(0, 1000).ToList();
            var queue = new ConcurrentQueue<int>();
            var threads= new List<Thread>();
            foreach (var i in entries)
            {
                var t = new Thread(() => queue.Enqueue(i));
                threads.Add(t);
                t.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            Assert.AreEqual(entries.Count, queue.Count);
            CollectionAssert.AreEquivalent(entries, queue.ToList());

            threads = new List<Thread>();
            
            foreach (var i in entries)
            {
                var t = new Thread(() => queue.Dequeue());
                threads.Add(t);
                t.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            Assert.AreEqual(0, queue.Count);
        }
    }
}