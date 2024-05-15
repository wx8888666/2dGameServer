using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Body
{
    [Serializable]
    public class SndLoadPrg
    {
        public long uid;
        public long roomId;
        public int progress;
    }

    [Serializable]
    public class NtfLoadPrg
    {
        public int[] progressArr;
    }
}
