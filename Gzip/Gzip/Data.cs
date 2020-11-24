using System.Collections.Generic;
using System.Threading;

namespace Gzip
{
    public class Data
    {
        public int Index { get; set; }
        public List<Thread> Threads { get; set; }
        public Thread ThreadThis { get; internal set; }
        public byte[] ReadByte { get; set; }
    }
}
