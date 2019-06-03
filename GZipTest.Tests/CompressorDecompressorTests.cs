using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GZipTest.Tests
{
    [TestClass]
    public class CompressorDecompressorTests
    {
        private const int KB = 1024;
        private const int MB = KB * 1024;
        private const int GB = MB * 1024;

        [TestMethod]
        [DataRow("gpl-3.0.txt", KB)]
        [DataRow("gpl-3.0.txt", MB)]
        [DataRow("gpl-3.0.txt", GB)]
        [DataRow("voina-i-mir.txt", KB)]
        [DataRow("voina-i-mir.txt", MB)]
        [DataRow("voina-i-mir.txt", GB)]
        public void CompressDecompress(string fileName, int chunkSize)
        {
            var compressedExtension = $".{chunkSize:0000000000}.gzt";
            var decompressedExtension = $".{chunkSize:0000000000}";
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"Resources\\{fileName}");
            var compressor = new Compressor(path, chunkSize, path + compressedExtension);
            var decompressor = new Decompressor(path + compressedExtension, path + decompressedExtension);

            compressor.Compress();
            decompressor.Decompress();

            Assert.AreEqual(File.ReadAllText(path), File.ReadAllText(path + decompressedExtension));
        }

        [TestMethod]
        public void HugeFile()
        {
            var compressedExtension = ".gzt";
            var decompressedExtension = ".1";
            var performance = new PerformanceCounter("Memory", "Available MBytes");
            var memory = (long)performance.NextValue();
            var size = memory * 2 * MB;
            Debug.WriteLine($"Generating file with size {size/MB} MB.");
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"{Path.GetTempPath()}\\gzip\\huge.txt");
            GenerateRandomFile(path, size);
            var compressor = new Compressor(path, 10 * MB, path + compressedExtension);
            var decompressor = new Decompressor(path + compressedExtension, path + decompressedExtension);

            Debug.WriteLine($"Compressing...");
            compressor.Compress();
            Debug.WriteLine($"Decompressing...");
            decompressor.Decompress();

            Debug.WriteLine($"Comparing...");
            AssertAreEqual(path, path + decompressedExtension);
        }

        private static void AssertAreEqual(string path1, string path2)
        {
            var chunkSize = sizeof(long);
            var first = new FileInfo(path1);
            var second = new FileInfo(path2);
            var chunksCount = (int)Math.Ceiling((double)first.Length / chunkSize);
            using (FileStream fs1 = first.OpenRead())
            using (FileStream fs2 = second.OpenRead())
            {
                byte[] one = new byte[chunkSize];
                byte[] two = new byte[chunkSize];

                for (int i = 0; i < chunksCount; i++)
                {
                    fs1.Read(one, 0, chunkSize);
                    fs2.Read(two, 0, chunkSize);

                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                        Assert.Fail($"Files differ somewhere between {i*chunkSize} and {(i+1)*chunkSize} bytes.");
                    if ((i * chunkSize) % GB == 0)
                    {
                        Debug.WriteLine($"Compared {i*chunkSize/GB} GB.");
                    }
                }
            }
        }

        private static void GenerateRandomFile(string path, long size)
        {
            if (File.Exists(path)) { return; }
            using (var file = File.OpenWrite(path))
            {
                for (long i = 0; i < size; i++)
                {
                    file.WriteByte((byte) (i % byte.MaxValue));
                    if (i % GB == 0)
                    {
                        Debug.WriteLine($"Generated {i/GB} GB.");
                    }
                }
            }
        }
    }
}