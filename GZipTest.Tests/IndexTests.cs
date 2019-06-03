using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GZipTest.Tests
{
    [TestClass]
    public class IndexTests
    {
        [TestMethod]
        public void IndexWriteRead()
        {
            var originalIndex = new Index(Enumerable.Range(0, 1000).Select(i => new IndexEntry{OriginalPosition = i, CompressedChunk = new ChunkDescriptor {Size = i * 2, Position = i * 3}}).ToList());
            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".tmp");
            File.Create(tempFile).Close();
            originalIndex.WriteToFile(tempFile);
            var indexFromFile = Index.ReadFromFile(tempFile);
            foreach (var indexEntry in indexFromFile)
            {
                Assert.IsTrue(originalIndex.Any(i =>
                    i.OriginalPosition == indexEntry.OriginalPosition &&
                    i.CompressedChunk.Size == indexEntry.CompressedChunk.Size &&
                    i.CompressedChunk.Position == indexEntry.CompressedChunk.Position &&
                    i.ToByteArray().SequenceEqual(indexEntry.ToByteArray())));
            }

            foreach (var indexEntry in originalIndex)
            {
                Assert.IsTrue(indexFromFile.Any(i =>
                    i.OriginalPosition == indexEntry.OriginalPosition &&
                    i.CompressedChunk.Size == indexEntry.CompressedChunk.Size &&
                    i.CompressedChunk.Position == indexEntry.CompressedChunk.Position &&
                    i.ToByteArray().SequenceEqual(indexEntry.ToByteArray())));
            }
            File.Delete(tempFile);
        }
    }
}