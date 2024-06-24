using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.DBModel
{
    [Serializable]
    public class MapConfigModel
    {
        public int Id { get; set; }
        public string RoleBornPoints { get; set; }
        public string EnemyBornPoints { get; set; }
        public int EnemySpawnTime { get; set; }
        public int EnemySpawnMaxCount { get; set; }
        public int SupplySpawnTime { get; set; }
        public int SupplySpawnMaxCount { get; set; }
    }
}
