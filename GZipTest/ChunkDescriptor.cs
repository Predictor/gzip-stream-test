using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    public class ChunkDescriptor
    {
        public long Position { get; set; }
        public long Size { get; set; }
    }
}
