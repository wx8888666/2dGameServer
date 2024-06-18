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
        public int SpiritNum { get; set; }
        public int LoveNum { get; set; }

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
    [Serializable]
    public class TaskChange
    {
        public long UId { get; set; }
        public string description { get; set; }
        public bool isCompleted { get; set; }
    }
}
