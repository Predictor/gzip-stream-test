using System.Collections.Generic;
using System.IO;

namespace GZipTest
{
    public class FilePartitioner
    {
        public static ICollection<ChunkDescriptor> CalculateChunks(string filePath, int maxPartitionSize)
        {
            var sourceChunks = new List<ChunkDescriptor>();
            var length = new FileInfo(filePath).Length;
            for (long i = 0; i < length - maxPartitionSize; i += maxPartitionSize)
            {
                sourceChunks.Add(new ChunkDescriptor { Position = i, Size = maxPartitionSize });
            }
            var remainder = (int)(length - sourceChunks.Count * maxPartitionSize);
            if (remainder > 0)
            {
                sourceChunks.Add(new ChunkDescriptor { Position = sourceChunks.Count * maxPartitionSize, Size = remainder });
            }

            return sourceChunks;
        }
    }
}
