using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest
{
    public class Compressor
    {
        private const int MaxBytes = 1024 * 1024 * 1024;
        private SemaphoreSlim srcSem = new SemaphoreSlim(1, 1);
        private SemaphoreSlim compSem = new SemaphoreSlim(1, 1);
        private SemaphoreSlim targetSem = new SemaphoreSlim(1, 1);
        private SemaphoreSlim queueLenghtSem;
        private SemaphoreSlim fileWriteSem = new SemaphoreSlim(1, 1);
        private Queue<ChunkDescriptor> sourceChunks = new Queue<ChunkDescriptor>();
        private Queue<CompressedChunk> compressedChunks = new Queue<CompressedChunk>();
        private Index index = new Index();
        private string sourcePath;
        private readonly int chunkSize;
        private readonly string targetPath;
        private int maxQueueLenght;
        private int uncompressedChunksCount = 0;

        public Compressor(string sourcePath, int chunkSize, string targetPath)
        {
            this.sourcePath = sourcePath;
            this.chunkSize = chunkSize;
            this.targetPath = targetPath;
        }

        public void Compress()
        {
            this.CalculateChunks();
            if(sourceChunks.Count == 0)
            {
                return;
            }

            var threads = new List<Thread>();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                threads.Add(StartCompressorThread());
            }

            while (uncompressedChunksCount > 0 || compressedChunks.Count > 0)
            {
                DequeueAndWriteNextChunk();
            }

            index.WriteToFile(targetPath);
        }

        private Thread StartCompressorThread()
        {
            var thread = new Thread(new ThreadStart(() =>
            {
                while (sourceChunks.Count > 0)
                {
                    ProcessAndEnqueueChunk();
                }
            }));
            thread.Start();
            return thread;
        }


        private void ProcessAndEnqueueChunk()
        {
            queueLenghtSem.Wait();
            var chunkDescriptor = GetNextChunk();
            if(chunkDescriptor == null)
            {
                return;
            }

            var memoryStream = new MemoryStream();
            var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress);
            var buffer = ReadChunk(chunkDescriptor);
            buffer.CopyTo(gzipStream);
            EnqueueCompressedChunk(new CompressedChunk { ChunkDescriptor = chunkDescriptor, CompressedStream = memoryStream });
        }

        private void DequeueAndWriteNextChunk()
        {
            fileWriteSem.Wait();
            try
            {
                if (compressedChunks.Count == 0)
                {
                    return;
                }

                var sourceChunk = DequeueCompressedChunk();
                if (sourceChunk == null)
                {
                    return;
                }
                var position = File.Exists(targetPath) ? new FileInfo(targetPath).Length : 0;
                var targetChunk = new ChunkDescriptor { Position = position, Size = sourceChunk.CompressedStream.Length };
                using (FileStream outFile = new FileStream(targetPath, FileMode.Append, FileAccess.Write))
                {
                    sourceChunk.CompressedStream.Seek(0, SeekOrigin.Begin);
                    sourceChunk.CompressedStream.CopyTo(outFile);
                }

                AddIndexEntry(sourceChunk.ChunkDescriptor, targetChunk);
                queueLenghtSem.Release();
            }
            finally
            {
                fileWriteSem.Release();
            }
        }

        private void CalculateChunks()
        {
            this.maxQueueLenght = MaxBytes / chunkSize;
            queueLenghtSem = new SemaphoreSlim(maxQueueLenght, maxQueueLenght);
            long length = new FileInfo(sourcePath).Length;
            for(long i = 0; i < length - chunkSize; i += chunkSize)
            {
                sourceChunks.Enqueue(new ChunkDescriptor { Position = i, Size = chunkSize });
                uncompressedChunksCount++;
            }
            var remainder = length - sourceChunks.Count * chunkSize;
            if (remainder > 0)
            {
                sourceChunks.Enqueue(new ChunkDescriptor { Position = sourceChunks.Count * chunkSize, Size = remainder });
                uncompressedChunksCount++;
            }
        }

        private ChunkDescriptor GetNextChunk()
        {
            srcSem.Wait();
            try
            {
                if(sourceChunks.Count == 0)
                {
                    return null;
                }

                return sourceChunks.Dequeue();
            }
            finally
            {
                srcSem.Release();
            }
        }

        private MemoryStream ReadChunk(ChunkDescriptor descriptor)
        {
            var buffer = new byte[descriptor.Size];
            using (var fileStream = File.OpenRead(sourcePath))
            {
                fileStream.Seek(descriptor.Position, SeekOrigin.Begin);
                for(long i =0; i < descriptor.Size; i++)
                {
                    buffer[i] = (byte)fileStream.ReadByte();
                }
            }

            return new MemoryStream(buffer);
        }

        private void EnqueueCompressedChunk(CompressedChunk chunk)
        {
            compSem.Wait();
            try
            {
                compressedChunks.Enqueue(chunk);
                uncompressedChunksCount--;
            }
            finally
            {
                compSem.Release();
            }
        }

        private CompressedChunk DequeueCompressedChunk()
        {
            compSem.Wait();
            try
            {
                if (compressedChunks.Count == 0)
                {
                    return null;
                }

                return compressedChunks.Dequeue();
            }
            finally
            {
                compSem.Release();
            }
        }

        private void AddIndexEntry(ChunkDescriptor sourceDescriptor, ChunkDescriptor targetDescriptor)
        {
            var entry = new IndexEntry { CompressedChunk = targetDescriptor, OriginalPosition = sourceDescriptor.Position };
            targetSem.Wait();
            try
            {
                index.Add(entry);
            }
            finally
            {
                targetSem.Release();
            }
        }
    }
}
