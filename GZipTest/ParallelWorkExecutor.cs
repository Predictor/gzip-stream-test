using System;
using System.Collections.Generic;
using System.Threading;

namespace GZipTest
{
    public class ParallelWorkExecutor
    {
        private readonly Action work;
        private readonly int numberOfThreads;
        private readonly List<Thread> threads;
        private Func<bool> exitCondition;

        public ParallelWorkExecutor(Action work, Func<bool> exitCondition, int numberOfThreads)
        {
            this.work = work;
            this.exitCondition = exitCondition;
            this.numberOfThreads = numberOfThreads;
            this.threads = new List<Thread>();
        }

        public void Start()
        {
            for (int i = 0; i < numberOfThreads; i++)
            {
                threads.Add(StartThread());
            }
        }

        public void Wait()
        {
            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        public void Cancell(bool force)
        {
            if (!force)
            {
                this.exitCondition = () => true;
                Wait();
                return;
            }

            foreach (var thread in threads)
            {
                thread.Abort();
            }
        }

        private Thread StartThread()
        {
            var thread = new Thread(() =>
            {
                while (!exitCondition())
                {
                    work();
                    Thread.Sleep(1);
                }
            });
            thread.Start();
            return thread;
        }

    }
}
