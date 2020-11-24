using System;

namespace Gzip
{
    class Constants
    {
        public static int processorCount = Environment.ProcessorCount;
        public static int blockSize = 1048576;
        public static int decompressedBlockSize = 264684;
    }
}
