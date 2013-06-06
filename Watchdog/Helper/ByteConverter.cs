using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Watchdog.Helper
{
    class ByteConverter
    {
        public static long GetGigaBytesFromBytes(long bytes)
        {
            return bytes/1024/1024/1024;
        }
    }
}
