using Protocol.Body;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.DBModel
{
    public enum SupplyTypeEnum
    {
        Weapon,
        Box,
        Shoe,
        HP,
        Def,
        Bullet,
    }

    [Serializable]
    public class SupplyState : ActorObject
    {
        public SupplyTypeEnum supplyType;//补给类型

        public WeaponObject weapon;//武器信息
        public int currentHP;//加血同步当前血量
        public int currentBullet;//加子弹同步当前子弹数量

        public int shoe;//加移速
        public int hp;//加血
        public int def;//加防御力
        public int bullet;//加子弹
    }
    [Serializable]
    public class NtfSpawnSupply
    {
        public SupplyState supplyState;
    }
}
