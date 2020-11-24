using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace Gzip
{
    public class DecompressClass
    {
        private static byte[] dataArray = new byte[Constants.blockSize];
        private static byte[][] decompressedData = new byte[Constants.processorCount][];
        public static string inFileName = "";
        public static string outFileName = "";

        private static long check = 0;

        private static int index = 0;
        private static List<Thread> threads = new List<Thread>();

        private static long readFinish = 0;

        private static Thread writeThread;
        public static void Write(object fileName)
        {
            using (FileStream outFile = new FileStream(fileName.ToString(), FileMode.Create, FileAccess.Write))
            {
                Console.WriteLine("Write.");
                long checkMini = 0;
                int countThreads = 0;
                do
                {
                    lock (threads)
                    {
                        countThreads = threads.Count;
                        if (countThreads > 0 && !threads.Any(x => x.ThreadState != ThreadState.Stopped))
                        {
                            foreach (var data in decompressedData.Where(x => x != null && x[0] != 0))
                            {
                                outFile.Write(data, 0, Constants.blockSize);
                            }
                            ActionsWithArray.Clear(decompressedData);
                            threads.Clear();
                        }
                    }
                    checkMini = Interlocked.Read(ref readFinish);
                } while (checkMini != 1 || countThreads > 0);
                outFile.Close();
                Console.WriteLine("Write end.");
            }
        }
        static public int Decompress(string inFileName, string outFileName)
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

            Console.WriteLine("Decompress.");
            do
            {
                lock (threads)
                {
                    if (threads.Count < Constants.processorCount)
                    {
                        if ((inFile.Length - inFile.Position) < Constants.decompressedBlockSize)
                        {
                            dataArray = new byte[inFile.Length - inFile.Position];
                            inFile.Read(dataArray, 0, (int)(inFile.Length - inFile.Position));
                        }
                        else
                        {
                            dataArray = new byte[Constants.decompressedBlockSize];
                            inFile.Read(dataArray, 0, Constants.decompressedBlockSize);
                            var thread = new Thread(DecompressBlock);
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
            }
            while (inFile.Position < inFile.Length);
            Interlocked.Exchange(ref readFinish, 1);
            while (writeThread.ThreadState != ThreadState.Stopped)
            {
            }
            Console.WriteLine("Decompress end.");
            return 0;
        }
        public static void DecompressBlock(object param)
        {
            var info = (Data)param;
            using (MemoryStream input = new MemoryStream(info.ReadByte))
            {
                decompressedData[info.Index % Constants.processorCount] = new byte[Constants.blockSize];
                using (GZipStream ds = new GZipStream(input, CompressionMode.Decompress))
                {
                    ds.Read(decompressedData[info.Index % Constants.processorCount], 0, decompressedData[info.Index % Constants.processorCount].Length);
                }
            }
            Interlocked.Increment(ref check);
        }
    }
}
