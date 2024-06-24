using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Body
{
    [Serializable]
    public class StateRoot
    {
        /// <summary>
        /// 当前实体的id（全局唯一）
        /// </summary>
        public long id;

        public NetVector2 pos = NetVector2.Zero;
        public NetVector2 dir = NetVector2.Zero;

    }

    public enum EntityStateEnum
    {
        Alive,
        Dying,
        Dead,
    }

    public enum EntityTypeEnum
    {
        Role,
        Enemy,
    }
    [Serializable]
    public class HumanStateRoot : StateRoot
    {
        /// <summary>
        /// 实体的名称（不是用户名）
        /// </summary>
        public string name;
        /// <summary>
        /// 当前实体的状态（存活、死亡）
        /// </summary>
        public EntityStateEnum entityState;
        /// <summary>
        /// 当前实体类型（角色、小怪）
        /// </summary>
        public EntityTypeEnum entityType;
        public float speed;
        public int hp;
        public int maxHp;
        public int def;
    }
    [Serializable]
    public class EnemyState : HumanStateRoot
    {
        public EnemyState()
        {
            entityState = EntityStateEnum.Alive;
            entityType = EntityTypeEnum.Enemy;
        }
    }
    [Serializable]
    public class NtfEnemySpawn
    {
        public EnemyState enemyState;
    }
    [Serializable]
    public class NtfSpawnEnemy
    {
        public EnemyState[] enemyStates;
    }
    [Serializable]
    public class NtfSyncEnemy
    {
        public EnemyState[] enemyStates;
    }
}
