using System;
using System.Diagnostics;
using System.Threading;

namespace GZipTest
{
    public static class MemoryLimiter
    {
        private const int MB = 1024 * 1024;

        public static void Wait()
        {
            while (!IsEnoughMemory())
            {
                Thread.Sleep(1000);
            }
        }

        public static bool IsEnoughMemory()
        {
            return Math.Max(Process.GetCurrentProcess().WorkingSet64, Process.GetCurrentProcess().PrivateMemorySize64) < AllowedMemoryBytes;
        }

        public static long AllowedMemoryBytes
        {
            get
            {
                var performance = new PerformanceCounter("Memory", "Available MBytes");
                var availableMemory = MB * (long) performance.NextValue();
                return (IntPtr.Size == 4) ? Math.Min(availableMemory / 2, 512 * MB) : availableMemory / 2;
            }
        }
    }
}
