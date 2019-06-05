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
            source.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        public void Dispose()
        {
            source?.Dispose();
        }
    }
}
