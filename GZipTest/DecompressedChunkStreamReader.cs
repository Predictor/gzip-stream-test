﻿using System;
using System.IO;
using System.IO.Compression;

namespace GZipTest
{
    public class DecompressedChunkStreamReader : IDisposable
    {
        private readonly Stream source;

        public DecompressedChunkStreamReader(Stream source)
        {
            if (!source.CanRead || !source.CanSeek)
            {
                throw new ArgumentException(nameof(source));
            }
            this.source = source;
        }

        public byte[] Read(ChunkDescriptor chunkDescriptor)
        {
            source.Seek(chunkDescriptor.Position, SeekOrigin.Begin);
            var buffer = new byte[chunkDescriptor.Size];
            source.Read(buffer, 0, buffer.Length);
            using (var targetStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(new MemoryStream(buffer), CompressionMode.Decompress))
                {
                    gzipStream.CopyTo(targetStream);
                }

                return targetStream.ToArray();
            }
        }

        public void Dispose()
        {
            source.Dispose();
        }
    }
}