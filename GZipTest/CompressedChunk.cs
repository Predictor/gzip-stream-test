using System.IO;

namespace GZipTest
{
    public class CompressedChunk
    {
        private byte[] data;
        public ChunkDescriptor SourceChunkDescriptor { get; private set; }
        public int Length => data?.Length ?? 0;

        public static CompressedChunk FromSourceFile(string sourcePath, ChunkDescriptor descriptor)
        {
            var compressedChunk = new CompressedChunk {SourceChunkDescriptor = descriptor};
            using (var reader = new CompressedChunkStreamReader(File.OpenRead(sourcePath)))
            {
                compressedChunk.data = reader.Read(descriptor);
            }

            return compressedChunk;
        }

        public ChunkDescriptor AppendToFile(string targetPath)
        {
            var position = File.Exists(targetPath) ? new FileInfo(targetPath).Length : 0;
            var targetChunk = new ChunkDescriptor { Position = position, Size = this.Length };
            using (FileStream outFile = new FileStream(targetPath, FileMode.Append, FileAccess.Write))
            {
                outFile.Write(data, 0, data.Length);
            }

            return targetChunk;
        }
    }
}
