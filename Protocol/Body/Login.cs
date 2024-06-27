using Protocol.DBModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Body
{
    [Serializable]
    public class ReqLogin
    {
        public string username;
        public string password;
    }

    [Serializable]
    public class RspLogin
    {
        public long uid;
        public User user;
        public LoginEnum loginEnum;
        public string error;
    }

    public enum LoginEnum
    {
        Success,
        Failed,
    }


    [Serializable]
    public class ReqLogout
    {
        public long uid;
    }
    //添加注册功能；
    [Serializable]
    public class ReqReg
    {
        public string Username;
        public string Password;
    }
    [Serializable]
    public class RspReg
    {
        public long Uid;
        public User user;
        public LoginEnum LoginEnum;
        public string Error;
    }
    [Serializable]
    public class UserTask
    {
        public string Description { get; set; }
        public string Reward { get; set; }
        public int Number { get; set; }
        public int RewardNumber { get; set; }
        public string Condition { get; set; }
        public bool IsCompleted { get; set; }

    }
    [Serializable]
    public class ReqTask
    {
        public long uid{ get; set; }
    }
    [Serializable]
    public class RspTask
    {
        
        public UserTask[] UserTask { get; set; }
    }

}
