using System;
using System.Diagnostics;
using System.Threading;

namespace GZipTest
{
    public class Compressor
    {
        private const int MaxBytes = 1024 * 1024 * 1024;
        private readonly string sourcePath;
        private readonly string targetPath;
        private readonly ConcurrentQueue<ChunkDescriptor> sourceChunks;
        private readonly LimitedConcurrentQueue<CompressedChunk> compressedChunks;
        private readonly Index index = new Index();
        private int uncompressedChunksCount;

        public Compressor(string sourcePath, int chunkSize, string targetPath)
        {
            this.sourcePath = sourcePath;
            this.targetPath = targetPath;
            this.compressedChunks = new LimitedConcurrentQueue<CompressedChunk>(Math.Max(MaxBytes / chunkSize, 1));
            this.sourceChunks = new ConcurrentQueue<ChunkDescriptor>(FilePartitioner.CalculateChunks(sourcePath, chunkSize));
            this.uncompressedChunksCount = sourceChunks.Count;
        }

        public void Compress()
        {
            if(uncompressedChunksCount == 0)
            {
                return;
            }

            new ParallelWorkExecutor(this.ProcessAndEnqueueChunk, () => sourceChunks.Count == 0, Math.Min(Environment.ProcessorCount, sourceChunks.Count)).Start();

            while (uncompressedChunksCount > 0 || compressedChunks.Count > 0)
            {
                DequeueAndWriteNextChunk();
            }

            index.WriteToFile(targetPath);
        }

        private void ProcessAndEnqueueChunk()
        {
            MemoryLimiter.Wait();
            ChunkDescriptor chunkDescriptor;
            try
            {
                chunkDescriptor = sourceChunks.Dequeue();
            }
            catch (InvalidOperationException)
            {
                // queue is empty
                return;
            }

            compressedChunks.Enqueue(CompressedChunk.FromSourceFile(sourcePath, chunkDescriptor));
            Interlocked.Decrement(ref uncompressedChunksCount);
            Debug.WriteLine($"Approximate uncompressed chunks count = {uncompressedChunksCount}.");
        }

        private void DequeueAndWriteNextChunk()
        {
            if (compressedChunks.Count == 0)
            {
                return;
            }

            var compressedChunk = compressedChunks.Dequeue();
            if (compressedChunk == null)
            {
                return;
            }

            var targetChunk = compressedChunk.AppendToFile(targetPath);
            index.Add(new IndexEntry{CompressedChunk = targetChunk, OriginalPosition = compressedChunk.SourceChunkDescriptor.Position});
        }
    }
}
