using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GZipTest.Tests
{
    [TestClass]
    public class FilePartitionerTests
    {
        [TestMethod]
        [DataRow("gpl-3.0.txt", 1, 35149)]
        [DataRow("gpl-3.0.txt", 35149, 1)]
        [DataRow("gpl-3.0.txt", 3514, 11)]
        [DataRow("voina-i-mir.txt", 4 * 1024 * 1024, 2)]
        [DataRow("voina-i-mir.txt", 8 * 1024 * 1024, 1)]
        public void CalculateChunks(string fileName, int maxPartitionSize, int chunksCount)
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"Resources\\{fileName}");
            Assert.AreEqual(chunksCount, FilePartitioner.CalculateChunks(path, maxPartitionSize).Count);
        }

        [TestMethod]
        [DataRow("gpl-3.0.txt")]
        [DataRow("voina-i-mir.txt")]
        public void GetRecommendedPartitionSize(string fileName)
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"Resources\\{fileName}");
            Assert.AreEqual(new FileInfo(path).Length / Environment.ProcessorCount, FilePartitioner.GetRecommendedChunkSize(path));
        }
    }
}
