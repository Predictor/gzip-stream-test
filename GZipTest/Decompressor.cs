using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
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

            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }

            this.indexQueue = new ConcurrentQueue<IndexEntry>(index);
            compressedChunksCount = index.Length;
            this.decompressedChunks = new ConcurrentDictionary<long, byte[]>();
            var parallelism = index.Length > 1 ? (int) Math.Min(Math.Max(MemoryLimiter.AllowedMemoryBytes / index[1].OriginalPosition, 1), Environment.ProcessorCount) : 1;
            if (parallelism == 1)
            {
                SequentialDecompress(index);
                return;
            }
            ParallelDecompress(parallelism, index);
        }

        private void ParallelDecompress(int parallelism, IndexEntry[] index)
        {
            new ParallelWorkExecutor(ProcessChunk, () => indexQueue.Count == 0, parallelism).Start();
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

        private void SequentialDecompress(IEnumerable<IndexEntry> index)
        {
            using (var outFile = new FileStream(targetPath, FileMode.OpenOrCreate, FileAccess.Write))
            using (var inFile = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
            {
                foreach (var entry in index)
                {
                    inFile.Seek(entry.CompressedChunk.Position, SeekOrigin.Begin);
                    var buffer = new byte[entry.CompressedChunk.Size];
                    for (int i = 0; i < entry.CompressedChunk.Size; i++)
                    {
                        buffer[i] = (byte) inFile.ReadByte();
                    }

                    using (var compressedStream = new MemoryStream(buffer))
                    {
                        using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                        {
                            gzipStream.CopyTo(outFile);
                        }
                    }
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
