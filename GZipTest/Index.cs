﻿namespace GZipTest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    public class Index : IEnumerable<IndexEntry>
    {
        private List<IndexEntry> entries;

        public Index()
        {
            entries = new List<IndexEntry>();
        }

        public void Add(IndexEntry entry)
        {
            entries.Add(entry);
        }

        public static Index ReadFromFile(string fileName)
        {
            var index = new Index();
            long totalLenght = new FileInfo(fileName).Length;
            using (FileStream inFile = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                inFile.Seek(-sizeof(long), SeekOrigin.End);
                var longBuffer = new byte[sizeof(long)];
                for (int i = 0; i < sizeof(long) / sizeof(byte); i++)
                {
                    longBuffer[i] = (byte)inFile.ReadByte();
                }

                var compressedFileSize = BitConverter.ToInt64(longBuffer, 0);
                inFile.Seek(compressedFileSize, SeekOrigin.Begin);
                for (int i = 0; i < totalLenght - compressedFileSize - longBuffer.Length; i += IndexEntry.SizeInBytes)
                {
                    var entryBuffer = new byte[IndexEntry.SizeInBytes];
                    for (int j = 0; j < IndexEntry.SizeInBytes; j++)
                    {
                        entryBuffer[j] = (byte)inFile.ReadByte();
                    }
                    index.Add(new IndexEntry(entryBuffer));
                }
            }
            return index;
        }

        public void WriteToFile(string fileName)
        {
            long length = new FileInfo(fileName).Length;
            using (FileStream outFile = new FileStream(fileName, FileMode.Append, FileAccess.Write))
            {
                foreach (var entry in entries)
                {
                    var bytes = entry.ToByteArray();
                    outFile.Write(bytes, 0, bytes.Length);
                }

                outFile.Write(BitConverter.GetBytes(length), 0, sizeof(long));
            }
        }

        public IEnumerator<IndexEntry> GetEnumerator()
        {
            return ((IEnumerable<IndexEntry>)entries).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<IndexEntry>)entries).GetEnumerator();
        }
    }
}