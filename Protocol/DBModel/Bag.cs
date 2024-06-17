using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.DBModel
{
    [Serializable]
    public class Bag
    {
        public long UId { get; set; }

        public int GoldCoinsNum { get; set; }

        public int MasonryNum { get; set; }

    }
    
    [Serializable]
    public class ReqBag
    {
        public long UId { get; set;}
    }
    [Serializable]
    public class RspBag
    {
        public Bag bag { get; set; }
    }
}
