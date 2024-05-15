using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.DBModel
{
    [Serializable]
    public class Account
    {
        public long Id { get; set; }//唯一id
        public string Username {  get; set; }//用户名
        public string Password {  get; set; }//密码
    }
}
