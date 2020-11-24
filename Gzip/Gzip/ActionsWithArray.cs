using System;

namespace Gzip
{
    public class ActionsWithArray
    {
        public static void Clear(byte[][] array)
        {
            lock (array)
            {
                foreach (var data in array)
                {
                    if (data != null)
                        Array.Clear(data, 0, data.Length);
                }
            }
        }
    }
}
