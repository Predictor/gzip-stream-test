namespace GZipTest
{
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    public class Decompressor
    {
        private string sourcePath;
        private string targetPath;

        public Decompressor(string sourcePath, string targetPath)
        {
            this.sourcePath = sourcePath;
            this.targetPath = targetPath;
        }

        public void Decompress()
        {
            var index = Index.ReadFromFile(sourcePath).OrderBy(e => e.OriginalPosition);
            using(var outFile = new FileStream(targetPath, FileMode.OpenOrCreate, FileAccess.Write))
            using (var inFile = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
            {
                foreach (var entry in index)
                {
                    inFile.Seek(entry.CompressedChunk.Position, SeekOrigin.Begin);
                    var buffer = new byte[entry.CompressedChunk.Size];
                    for (int i = 0; i < entry.CompressedChunk.Size; i++)
                    {
                        buffer[i] = (byte)inFile.ReadByte();
                    }
                    var compressedStream = new MemoryStream(buffer);
                    var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
                    gzipStream.CopyTo(outFile);
                }
            }
        }

    }
}
