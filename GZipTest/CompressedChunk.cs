using System;
using System.IO;

namespace GZipTest
{
    public class CompressedChunk : IDisposable
    {
        public ChunkDescriptor ChunkDescriptor { get; set; }
        public MemoryStream CompressedStream { get; set; }

        public void Dispose()
        {
            CompressedStream?.Dispose();
            CompressedStream = null;
        }
    }
}
