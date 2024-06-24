using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer.Helpers;
using Protocol.DBModel;

namespace _2DSurviveGameServer._03Svc
{
    public class ResSvc : SvcRoot<ResSvc>
    {
        private readonly string mapConfigPath = AppContext.BaseDirectory + "Config/MapConfig.csv";
        public override void Init()
        {
            base.Init();
            if (File.Exists(mapConfigPath))
            {
                List<MapConfigModel> mapConfigList = new List<MapConfigModel>();
                using (StreamReader sr = new StreamReader(mapConfigPath))
                {
                    sr.ReadLine();
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        string[] sp = line.Split(',');

                        mapConfigList.Add(new MapConfigModel
                        {
                            Id = int.Parse(sp[0]),
                            RoleBornPoints = sp[1],
                            EnemyBornPoints = sp[2],
                            EnemySpawnTime = int.Parse(sp[3]),
                            EnemySpawnMaxCount = int.Parse(sp[4]),
                            SupplySpawnTime = int.Parse(sp[5]),
                            SupplySpawnMaxCount = int.Parse(sp[6]),
                        });

                        line = sr.ReadLine();
                    }
                }

                //先清空配置表
                SqlSugarHelper.Db.Deleteable<MapConfigModel>().ExecuteCommand();
                //插入新的配置表
                SqlSugarHelper.Db.Insertable(mapConfigList).ExecuteCommand();
            }
            else
            {
                this.Error("MapConfig不存在！");
            }

        }
        private Dictionary<int, MapConfigModel> mapConfigCacheDic = new Dictionary<int, MapConfigModel>();
        public MapConfigModel GetMapConfigById(int id)
        {
            if (mapConfigCacheDic.ContainsKey(id))
            {
                return mapConfigCacheDic[id];
            }
            var mapCfg = SqlSugarHelper.Db.Queryable<MapConfigModel>().First(p => p.Id == id);
            mapConfigCacheDic.Add(id, mapCfg);
            return mapCfg;
        }
    }
    
}
