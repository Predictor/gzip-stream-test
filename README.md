# gzip-stream-test
Test CLI app to compress and decompress files with stream [GZipStream](https://docs.microsoft.com/en-us/dotnet/api/system.io.compression.gzipstream).

## Prerequisites
The project was created with the following tools. Older versions may also work.
 - Visual studio 2019
 - .Net Framework 4.7.2

## Comand line arguments
To compress a file run:

_**GZipTest.exe** compress [path_to_source_file] [path_to_compressed_file]_
- _path_to_source_file_ must point to an existing file
- _path_to_compressed_file_ must point to a new file (existing file won't be overwritten)

To decompress previously compressed file run:

_**GZipTest.exe** decompress [path_to_compressed_file] [path_to_decompressed_file]_
- _path_to_compressed_file_ must point to an existing file
- _path_to_decompressed_file_ must point to a new file (existing file won't be overwritten)

## Main classes
- [ChunkDescriptor.cs](https://github.com/Predictor/gzip-stream-test/blob/master/GZipTest/ChunkDescriptor.cs) - defines offset and size of a file chunk
- [ChunkStreamReader.cs](https://github.com/Predictor/gzip-stream-test/blob/master/GZipTest/ChunkStreamReader.cs) - reads a chunk from a seekable stream
- [CompressedChunk.cs](https://github.com/Predictor/gzip-stream-test/blob/master/GZipTest/CompressedChunk.cs) - artifact of _CompressedChunkStreamReader_; contains a byte array with compressed data and _ChunkDescriptor_ of original chunk.
- [CompressedChunkStreamReader.cs](https://github.com/Predictor/gzip-stream-test/blob/master/GZipTest/CompressedChunkStreamReader.cs) - reads a chunk from a seekable stream and compresses it with [GZipStream](https://docs.microsoft.com/en-us/dotnet/api/system.io.compression.gzipstream).
- [Compressor.cs](https://github.com/Predictor/gzip-stream-test/blob/master/GZipTest/Compressor.cs) - compresses a given file with [GZipStream](https://docs.microsoft.com/en-us/dotnet/api/system.io.compression.gzipstream).
- [ConcurrentDictionary.cs](https://github.com/Predictor/gzip-stream-test/blob/master/GZipTest/ConcurrentDictionary.cs) - thread safe wrapper over [Dictionary<TKey,TValue>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2).
- [ConcurrentQueue.cs](https://github.com/Predictor/gzip-stream-test/blob/master/GZipTest/ConcurrentQueue.cs) - thread safe wrapper over [Queue<T>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.queue-1).
- [DecompressedChunkStreamReader.cs](https://github.com/Predictor/gzip-stream-test/blob/master/GZipTest/DecompressedChunkStreamReader.cs) - reads a chunk from a seekable stream and decompresses it with [GZipStream](https://docs.microsoft.com/en-us/dotnet/api/system.io.compression.gzipstream).
- [Decompressor.cs](https://github.com/Predictor/gzip-stream-test/blob/master/GZipTest/Decompressor.cs) - decompresses a given file with [GZipStream](https://docs.microsoft.com/en-us/dotnet/api/system.io.compression.gzipstream).
- [FilePartitioner.cs](https://github.com/Predictor/gzip-stream-test/blob/master/GZipTest/Index.cs) - helper class to break a file into chunks.
- [Index.cs](https://github.com/Predictor/gzip-stream-test/blob/master/GZipTest/Index.cs) - compressed file index. Chunks are written in random order. The index is used to restore the original order during decompression.
- [IndexEntry.cs](https://github.com/Predictor/gzip-stream-test/blob/master/GZipTest/IndexEntry.cs) - describes a single compressed chunk within the index.
- [LimitedConcurrentQueue.cs](https://github.com/Predictor/gzip-stream-test/blob/master/GZipTest/LimitedConcurrentQueue.cs) - wrapper over [ConcurrentQueue.cs](https://github.com/Predictor/gzip-stream-test/blob/master/GZipTest/ConcurrentQueue.cs) with limited capacity, when capacity is reached, all Enqueue() calls are being locked until Dequeue() is called.
- [MemoryLimiter.cs](https://github.com/Predictor/gzip-stream-test/blob/master/GZipTest/MemoryLimiter.cs) - helper class to limit memory consumption.
- [ParallelWorkExecutor.cs](https://github.com/Predictor/gzip-stream-test/blob/master/GZipTest/ParallelWorkExecutor.cs) - simple parallel action executor.
- [Program.cs](https://github.com/Predictor/gzip-stream-test/blob/master/GZipTest/Program.cs) - main class of the program.