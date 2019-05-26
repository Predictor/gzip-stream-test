using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    class Program
    {
        private const string Usage = "compressing: GZipTest.exe compress [original file name] [archive file name]\n" +
                                     "decompressing: GZipTest.exe decompress[archive file name] [decompressing file name]";
        static int Main(string[] args)
        {
            if(args.Length != 3)
            {
                Console.WriteLine("Invalid argument count.");
                Console.WriteLine(Usage);
                return 1;
            }
            var inputFilePath = args[1];
            if (!File.Exists(inputFilePath))
            {
                Console.WriteLine("Input file does not exist.");
                return 1;
            }

            var outFilePath = args[2];
            try
            {
                Path.GetFileName(outFilePath);
            }
            catch
            {
                Console.WriteLine("Invalid output file name.");
                return 1;
            }

            if (File.Exists(outFilePath))
            {
                Console.WriteLine("Output file already exists. Specify a new file name.");
                return 1;
            }

            switch (args[0])
            {
                case "compress":
                    var compressor = new Compressor(inputFilePath, 1024*1024, outFilePath);
                    compressor.Compress();
                    break;
                case "decompress":
                    var decompressor = new Decompressor(inputFilePath, outFilePath);
                    decompressor.Decompress();
                    break;
                default:
                    Console.WriteLine("Cannot parse arguments.");
                    Console.WriteLine(Usage);
                    return 1;
            }

                return 0;
        }
    }
}
