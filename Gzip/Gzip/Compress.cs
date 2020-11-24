using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace Gzip
{
    public class CompressClass
    {
        private static byte[] dataArray = new byte[Constants.blockSize];
        private static byte[][] compressedData = new byte[Constants.processorCount][];

        private static long check = 0;
        private static int index = 0;

        private static List<Thread> threads = new List<Thread>();

        private static long readFinish = 0;

        private static Thread writeThread;
        static public int Compress(string inFileName, string outFileName)
        {
            if (Constants.processorCount == 0)
            {
                Console.WriteLine("No processors.");
                return 1;
            }

            FileStream inFile = CreateFileStream.OpenStream(inFileName);

            if (inFile.Length == 0)
            {
                Console.WriteLine("File length is zero.");
                return 1;
            }
            Console.WriteLine("Compress.");
            do
            {
                lock (threads)
                {
                    if (threads.Count < Constants.processorCount)
                    {
                        if ((inFile.Length - inFile.Position) < Constants.blockSize)
                        {
                            dataArray = new byte[inFile.Length - inFile.Position];
                            inFile.Read(dataArray, 0, (int)(inFile.Length - inFile.Position));
                        }
                        else
                        {
                            dataArray = new byte[Constants.blockSize];
                            inFile.Read(dataArray, 0, Constants.blockSize);
                        }
                        var thread = new Thread(CompressBlock);
                        threads.Add(thread);
                        thread.Start(new Data
                        {
                            Index = index,
                            Threads = threads,
                            ThreadThis = thread,
                            ReadByte = dataArray
                        });
                        Interlocked.Increment(ref index);

                        if (writeThread is null)
                        {
                            writeThread = new Thread(Write);
                            writeThread.Start(outFileName);
                        }
                    }
                }
            }
            while (inFile.Position < inFile.Length);
            Interlocked.Exchange(ref readFinish, 1);
            while (writeThread.ThreadState != ThreadState.Stopped)
            {
            }
            Console.WriteLine("Compress end.");
            return 0;
        }
        public static void CompressBlock(object param)
        {
            var info = (Data)param;
            using (MemoryStream output = new MemoryStream(info.ReadByte.Length))
            {
                using (GZipStream compressionStream = new GZipStream(output, CompressionMode.Compress))
                {
                    compressionStream.Write(info.ReadByte, 0, info.ReadByte.Length);
                }
                byte[] Mass = new byte[Constants.blockSize];
                output.ToArray().CopyTo(Mass, 0);
                compressedData[info.Index % Constants.processorCount] = Mass;
                Interlocked.Increment(ref check);
            }
        }
        public static void Write(object fileName)
        {
            using (FileStream outFile = new FileStream(fileName.ToString(), FileMode.Create, FileAccess.Write))
            {
                Console.WriteLine("Write.");
                long localReadFinish = 0;
                int countThreads = 0;
                do
                {
                    lock (threads)
                    {
                        countThreads = threads.Count;
                        if (countThreads > 0 && !threads.Any(x => x.ThreadState != ThreadState.Stopped))
                        {
                            foreach (var data in compressedData.Where(x => x != null && x[0] != 0))
                            {
                                outFile.Write(data, 0, Constants.decompressedBlockSize);
                            }
                            ActionsWithArray.Clear(compressedData);
                            threads.Clear();
                        }
                    }
                    localReadFinish = Interlocked.Read(ref readFinish);
                } while (localReadFinish != 1 || countThreads > 0);
                outFile.Close();
                Console.WriteLine("Write end.");
            }
        }
    }
}
