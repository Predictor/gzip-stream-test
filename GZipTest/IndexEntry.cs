using System;
using System.Collections.Generic;

namespace GZipTest
{
    public class IndexEntry
    {
        public const int SizeInBytes = sizeof(long) * 3;

        private const int CompressedChunkPositionOffset = sizeof(long);

        private const int CompressedChunkSizeOffset = sizeof(long) * 2;

        public ChunkDescriptor CompressedChunk { get; set; }

        public long OriginalPosition { get; set; }

        public IndexEntry() { }

        public IndexEntry(byte[] bytes)
        {
            OriginalPosition = BitConverter.ToInt64(bytes, 0);
            CompressedChunk = new ChunkDescriptor { Position = BitConverter.ToInt64(bytes, CompressedChunkPositionOffset), Size = BitConverter.ToInt64(bytes, CompressedChunkSizeOffset) };
        }

        public byte[] ToByteArray()
        {
            var buffer = new List<byte>(SizeInBytes);
            buffer.AddRange(BitConverter.GetBytes(OriginalPosition));
            buffer.AddRange(BitConverter.GetBytes(CompressedChunk.Position));
            buffer.AddRange(BitConverter.GetBytes(CompressedChunk.Size));
            return buffer.ToArray();
        }
    }
}
