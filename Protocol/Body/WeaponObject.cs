using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Body
{
    [Serializable]
    public class WeaponObject : ActorObject
    {
        //武器资源
        public int assetId{ get; set; }

        public int damage { get; set; }

        public float rate { get; set; }

        public int magazinesCount { get; set; }

        public int maxMagazinesCount { get; set; }

        public int reserveMagazineCount { get; set; }

        public int maxReserveMagazineCount { get; set; }
    }
    [Serializable]
    public class NtfSpawnWeapon
    {
        public WeaponObject[] weaponObjectArr;
    }
    [Serializable]
    public class ReqWeaponFire
    {
        public long uid;
        public long roomId;
        public int weaponId;
        public NetVector2 startPos;
        public NetVector2 endPos;
    }
    [Serializable]
    public class RspWeaponFire
    {
        public bool isFireSuccess = false;//是否射击成功
        public int magCount;//当前弹夹子弹数量
        public int spareMagCount;//当前后备弹夹子弹数量
        public NetVector2 startPos;
        public NetVector2 endPos;
        public BulletState bulletState;
    }
    [Serializable]
    public class ReqPickupWeapon
    {
        public long roomId;
        public long uid; // 请求拾取武器的角色UID
        public long weaponId; // 请求拾取的武器ID
    }
    [Serializable]
    public class RspPickupWeapon
    {
        public bool isPickUpSuccess;
        public long uid; // 拾取武器的角色UID
        public long weaponId; // 被拾取的武器ID
    }
    [Serializable]
    public class BulletState
    {
        public long UId { get; set; }
        public long Id { get; set; }
        public NetVector2 StartPos { get; set; }
        public NetVector2 EndPos { get; set; }
        public NetVector2 Pos { get; set; }
        public float Speed { get; set; }
        public string BulletName { get; set; }
    }
    [Serializable]
    public class NtfBulletState
    {
        public long uid;
        public BulletState BulletState { get; set; }
    }
    
}
