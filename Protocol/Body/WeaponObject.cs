using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Body
{
    [Serializable]
    public class WeaponObject : ActorObject
    {
        //武器资源
        public int assetId;

        public int damage;

        public float rate;

        public int magazinesCount;

        public int maxMagazinesCount;

        public int reserveMagazineCount;

        public int maxReserveMagazineCount;
    }
    [Serializable]
    public class NtfSpawnWeapon
    {
        public WeaponObject[] weaponObjectArr;
    }
}
