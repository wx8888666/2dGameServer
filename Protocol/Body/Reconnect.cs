using Protocol.DBModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Body
{
    [Serializable]
    public class ReqClientState
    {
        public long uid;
    }

    [Serializable]
    public class RspClientState
    {
        public ClientStateEnum clientState;
        public RoomConfirmState roomConfirmState;
        public RoomLoadState roomLoadState;
        public RoomFightState roomFightState;
    }


    [Serializable]
    public class SndEnterRoom
    {
        public long uid;
        public long roomId;
    }


    public enum ClientStateEnum
    {
        None,
        MatchingQueue,
        RoomConfirm,
        RoomLoad,
        RoomFight,
    }

    [Serializable]
    public class RoomConfirmState
    {
        public long roomId;
        public User[] userArr;
        public bool[] confirmArr;
    }

    [Serializable]
    public class RoomLoadState
    {
        public long roomId;
        public User[] userArr;
        public int[] progressArr;
    }

    [Serializable]
    public class RoomFightState
    {
        public long roomId;
        public User[] userArr;
        public RoleState[] roleStateArr;
    }
}
