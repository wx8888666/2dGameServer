using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer._02Sys.Room.Actors;
using _2DSurviveGameServer.Helpers;
using Protocol.Body;
using Protocol.DBModel;

namespace _2DSurviveGameServer._03Svc
{
    public class ResSvc : SvcRoot<ResSvc>
    {
        private readonly string weaponConfigPath ="data/WeaponConfig.csv";
        private List<WeaponObject> weaponConfigList;

        public override void Init()
        {
            base.Init();
            if (File.Exists(weaponConfigPath))
            {
                weaponConfigList = new List<WeaponObject>();
                using (StreamReader sr = new StreamReader(weaponConfigPath))
                {
                    sr.ReadLine();
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        string[] sp = line.Split(',');

                        weaponConfigList.Add(new WeaponObject
                        {
                            assetId = int.Parse(sp[0]),
                            rate = float.Parse(sp[2]),
                            magazinesCount = int.Parse(sp[3]),
                            maxMagazinesCount = int.Parse(sp[3]),
                            reserveMagazineCount = int.Parse(sp[4]),
                        });

                        line = sr.ReadLine();
                    }
                }
            
                SqlSugarHelper.Db.CodeFirst.InitTables<WeaponObject>();
                SqlSugarHelper.Db.Insertable(weaponConfigList).ExecuteCommand();
            }
        }

        public WeaponObject GetWeaponConfigById(int id)
        {
            return SqlSugarHelper.Db.Queryable<WeaponObject>().First(p => p.assetId == id);
        }

        public WeaponObject GetRandomWeaponConfig()
        {
            if (weaponConfigList != null && weaponConfigList.Count > 0)
            {
                Random random = new Random();
                int index = random.Next(weaponConfigList.Count);
                return weaponConfigList[index];
            }
            return null;
        }
    }

}
