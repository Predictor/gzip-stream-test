using System;
using System.Diagnostics;
using System.Threading;

namespace GZipTest
{
    using System.IO;
    using System.Linq;
    public class Decompressor
    {
        private readonly string sourcePath;
        private readonly string targetPath;
        private ConcurrentDictionary<long, byte[]> decompressedChunks;
        private ConcurrentQueue<IndexEntry> indexQueue;
        private int compressedChunksCount = 0;

        public Decompressor(string sourcePath, string targetPath)
        {
            this.sourcePath = sourcePath;
            this.targetPath = targetPath;
        }

        public void Decompress()
        {
            var index = Index.ReadFromFile(sourcePath).OrderBy(e => e.OriginalPosition).ToArray();
            if (index.Length == 0)
            {
                return;
            }

            this.indexQueue = new ConcurrentQueue<IndexEntry>(index);
            compressedChunksCount = index.Length;
            this.decompressedChunks = new ConcurrentDictionary<long, byte[]>();
            new ParallelWorkExecutor(ProcessChunk, () => indexQueue.Count == 0, Math.Min(Environment.ProcessorCount, index.Length)).Start();
            using (var outFile = new FileStream(targetPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                foreach (var entry in index)
                {
                    byte[] chunk;
                    while (!decompressedChunks.TryRemove(entry.OriginalPosition, out chunk))
                    {
                        Thread.Sleep(1);
                    }

                    outFile.Write(chunk, 0, chunk.Length);
                    outFile.Flush();
                }
            }
        }

        private void ProcessChunk()
        {
            MemoryLimiter.Wait();
            IndexEntry indexEntry;
            try
            {
                indexEntry = indexQueue.Dequeue();
            }
            catch (InvalidOperationException)
            {
                // queue is empty;
                return;
            }

            using (var reader = new DecompressedChunkStreamReader(new FileStream(sourcePath, FileMode.Open, FileAccess.Read)))
            {
                var buffer = reader.Read(indexEntry.CompressedChunk);
                decompressedChunks.Add(indexEntry.OriginalPosition, buffer);
            }

            Interlocked.Decrement(ref compressedChunksCount);
            Debug.WriteLine($"Approximate compressed chunks count = {compressedChunksCount}.");
        }

    }
}
