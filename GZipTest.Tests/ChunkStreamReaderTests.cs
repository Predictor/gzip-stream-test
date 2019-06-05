using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GZipTest.Tests
{
    [TestClass]
    public class ChunkStreamReaderTests
    {
        [TestMethod]
        public void ReadChunk()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"Resources\\gpl-3.0.txt");
            string text = File.ReadAllText(path);
            var source = File.OpenRead(path);
            var chunk = new ChunkDescriptor {Position = 0, Size = 100};
            var reader = new ChunkStreamReader(source);
            var target = new MemoryStream(reader.Read(chunk));
            target.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(100, target.Length);
            var targetReader = new StreamReader(target);
            Assert.AreEqual(text.Substring(0, 100), targetReader.ReadToEnd());
        }

        [TestMethod]
        public void CompressDecompress()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"Resources\\gpl-3.0.txt");
            string text = File.ReadAllText(path);
            var chunk = new ChunkDescriptor {Position = 0, Size = text.Length};
            byte[] compressed;
            using (var r = new CompressedChunkStreamReader(File.OpenRead(path)))
            {
                compressed = r.Read(chunk);
            }
            var compressedStream = new MemoryStream(compressed);

            Stream decompressedStream;
            using (var r = new DecompressedChunkStreamReader(compressedStream))
            {
                decompressedStream = new MemoryStream(r.Read(new ChunkDescriptor { Position = 0, Size = compressed.Length }));
            }

            var decompressedText = new StreamReader(decompressedStream).ReadToEnd();
            Assert.AreEqual(text, decompressedText);
        }
    }
}
