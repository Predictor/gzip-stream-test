using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GZipTest
{
    public class CompressedChunkStreamReader : IDisposable
    {
        private readonly Stream source;

        public CompressedChunkStreamReader(Stream source)
        {
            this.source = source;
        }

        public byte[] Read(ChunkDescriptor chunkDescriptor)
        {
            using (var targetStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(targetStream, CompressionMode.Compress))
                {
                    var reader = new ChunkStreamReader(source);
                    var chunk = reader.Read(chunkDescriptor);
                    gzipStream.Write(chunk, 0, chunk.Length);
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
