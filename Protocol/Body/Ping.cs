using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Body
{

    [Serializable]
    public class ReqPing
    {
        public int pid;
        public long uid;
    }

    [Serializable]
    public class RspPing
    {
        public int pid;
    }
}
