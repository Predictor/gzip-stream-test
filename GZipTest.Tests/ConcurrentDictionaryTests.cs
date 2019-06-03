using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GZipTest.Tests
{
    [TestClass]
    public class ConcurrentDictionaryTests
    {
        [TestMethod]
        public void ConcurrentAddAndRemove()
        {
            var entries = Enumerable.Range(0, 1000).ToList();
            var dict = new ConcurrentDictionary<int, int>();
            var threads= new List<Thread>();
            foreach (var i in entries)
            {
                var t = new Thread(() => dict.Add(i, i));
                threads.Add(t);
                t.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            Assert.AreEqual(entries.Count, dict.Count);
            CollectionAssert.AreEquivalent(entries, dict.Select(d=>d.Key).ToList());

            threads = new List<Thread>();
            
            foreach (var i in entries)
            {
                var t = new Thread(() => dict.TryRemove(i, out _));
                threads.Add(t);
                t.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            Assert.AreEqual(0, dict.Count);
        }
    }
}
