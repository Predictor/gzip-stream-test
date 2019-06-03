using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GZipTest
{
    public class Compressor
    {
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
            this.compressedChunks =  new LimitedConcurrentQueue<CompressedChunk>(Math.Max((int)Math.Min(MemoryLimiter.AllowedMemoryBytes / chunkSize, int.MaxValue), 1));
            this.sourceChunks = new ConcurrentQueue<ChunkDescriptor>(FilePartitioner.CalculateChunks(sourcePath, chunkSize));
            this.uncompressedChunksCount = sourceChunks.Count;
        }

        public void Compress()
        {
            if(uncompressedChunksCount == 0)
            {
                return;
            }

            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
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
                Thread.Sleep(1);
                return;
            }

            CompressedChunk compressedChunk;
            try
            {
                compressedChunk = compressedChunks.Dequeue();
            }
            catch (InvalidOperationException)
            {
                // queue is empty
                Thread.Sleep(1);
                return;
            }

            var targetChunk = compressedChunk.AppendToFile(targetPath);
            index.Add(new IndexEntry{CompressedChunk = targetChunk, OriginalPosition = compressedChunk.SourceChunkDescriptor.Position});
        }
    }
}
