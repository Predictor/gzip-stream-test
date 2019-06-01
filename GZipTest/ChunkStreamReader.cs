using System;
using System.IO;

namespace GZipTest
{
    public class ChunkStreamReader :IDisposable
    {
        private readonly Stream source;

        public ChunkStreamReader(Stream source)
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
            for (long i = 0; i < chunkDescriptor.Size; i++)
            {
                buffer[i] = (byte) source.ReadByte();
            }

            return buffer;
        }

        public void Dispose()
        {
            source?.Dispose();
        }
    }
}
