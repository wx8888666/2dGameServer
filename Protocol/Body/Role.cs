using Protocol.DBModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Body
{
    [Serializable]
    public class ReqCreateRole
    {
        public long uid;
        public int roleId;
        public string roleName;
        public string head;
    }

    [Serializable]
    public class RspCreateRole
    {
        public User user;
        public CreateRoleEnum createRoleEnum;
        public string error;
    }

    public enum CreateRoleEnum
    {
        Success,
        Failed
    }

    [Serializable]
    public class ActorObject
    {
        public long id;

        public NetVector2 pos;
    }

    [Serializable]
    public class RoleState:ActorObject
    {
        public long uid;

        public string roleName;

        public int speed;

        public int hp;

        public int maxHp;

        public int def;

        public NetVector2 dir;

        public NetVector2 mousePos;

        public int exp;

        public int maxExp;

        public bool isKing = false;

        public WeaponObject weaponObject;
    }
    [Serializable]
    public class WeaponObject : ActorObject
    {
        public int assetId;

        public int damage;

        public float rate;

        public int magazinesCount;

        public int maxMagazinesCount;

        public int reserveMagazineCount;

        public int maxReserveMagazineCount;
    }

    [Serializable]
    public class NtfSpawnRole
    {
        public RoleState[] roleStates;
    }

    [Serializable]
    public class NtfSyncRole
    {
        public RoleState [] roleStates;
    }

    [Serializable]
    public class SndRoleState
    {
        public long uid;
        public long roomId;
        public RoleState roleState;
    }
   
}
