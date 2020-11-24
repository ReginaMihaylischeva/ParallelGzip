using System;
using System.IO.Compression;

namespace Gzip
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter an arguments.");
                return 1;
            }

            if (args[0] == CompressionMode.Compress.ToString())
            {
                if (args[1] != null || args[2] != null)
                {
                    CompressClass.Compress(args[1], args[2]);
                    return 0;
                }
                Console.WriteLine("Please enter a file name.");
                return 1;
            }

            if (args[0] == CompressionMode.Decompress.ToString())
            {
                if (args[1] != null || args[2] != null)
                {
                    DecompressClass.Decompress(args[1], args[2]);
                    return 0;
                }
                Console.WriteLine("Please enter a file name.");
                return 1;
            }
            Console.WriteLine("Please enter a valid compression mode.");
            return 1;
        }
    }
}
