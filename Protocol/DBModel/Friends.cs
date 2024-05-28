using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.DBModel
{
    [Serializable]
    public class Friends
    {
        public long UId { get; set; }
        public long FriendUId { get; set; }
    }
}
