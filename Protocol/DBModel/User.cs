using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.DBModel
{
    [Serializable]
    public class User
    {
        public long Id { get; set; }
        public long UId { get; set; }
        public int RoleId {  get; set; }
        public string RoleName { get; set; }
        public string Head {  get; set; }
    }
}
