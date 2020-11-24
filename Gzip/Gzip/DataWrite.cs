using System.Collections.Generic;
using System.Threading;

namespace Gzip
{
    internal class DataWrite
    {
        public long readFinish { get;  set; }
        public List<Thread> threads { get; internal set; }
        public string fileName { get; internal set; }
        public byte[][] Data { get; internal set; }
        public int blockSize { get; internal set; }
    }
}