using System;
using System.Threading;

namespace GZipTest
{
    using System.IO;
    using System.Linq;
    public class Decompressor
    {
        private const int MaxBytes = 1024 * 1024 * 1024;
        private readonly string sourcePath;
        private readonly string targetPath;
        private LimitedConcurrentSortedList<long, byte[]> decompressedChunks;
        private ConcurrentQueue<IndexEntry> indexQueue;
        private int compressedChunksCount = 0;

        public Decompressor(string sourcePath, string targetPath)
        {
            this.sourcePath = sourcePath;
            this.targetPath = targetPath;
        }

        public void Decompress()
        {
            var index = Index.ReadFromFile(sourcePath).OrderBy(e => e.OriginalPosition).ToList();
            if (index.Count == 0)
            {
                return;
            }

            this.indexQueue = new ConcurrentQueue<IndexEntry>(index);
            compressedChunksCount = index.Count;
            this.decompressedChunks = new LimitedConcurrentSortedList<long, byte[]>(Math.Min((int) (MaxBytes / index.First().CompressedChunk.Size), 1));
            new ParallelWorkExecutor(ProcessChunk, () => indexQueue.Count == 0, Math.Min(Environment.ProcessorCount, indexQueue.Count)).Start();
            using (var outFile = new FileStream(targetPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                while (decompressedChunks.Count > 0 || compressedChunksCount > 0)
                {
                    var chunk = decompressedChunks.GetNextAndRemove();
                    if (chunk == null)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    outFile.Write(chunk, 0, chunk.Length);
                }
            }
        }

        private void ProcessChunk()
        {
            var indexEntry = indexQueue.Dequeue();
            using (var reader = new DecompressedChunkStreamReader(new FileStream(sourcePath, FileMode.Open, FileAccess.Read)))
            {
                var buffer = reader.Read(indexEntry.CompressedChunk);
                decompressedChunks.Add(indexEntry.OriginalPosition, buffer);
            }

            Interlocked.Decrement(ref compressedChunksCount);
        }

    }
}
