using _2DSurviveGameServer._01Common.Config;
using _2DSurviveGameServer._02Sys.Room.Actors;
using _2DSurviveGameServer._03Svc;
using _2DSurviveGameServer.Helpers;
using FarseerPhysics.Dynamics;
using GameEngine;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using Protocol;
using Protocol.Body;
using System.Diagnostics;
using Yitter.IdGenerator;

namespace _2DSurviveGameServer._02Sys.Room.FSM
{
    public class RoomStateFight : RoomStateBase
    {
        // 静态实例
        private static RoomStateFight _instance;
        public static RoomStateFight Instance => _instance;
        GameWorld gameWorld;
        List<RoleActor> roleActorList;
        List<BulletActor> bulletActorsList;
        CancellationTokenSource cancellationTokenSource;
        Dictionary<long, WeaponActor> weaponDic;
        List<MonsterActor> monsterActorList;
        private bool isEnd;
        static int Monsterid = 1;
        private JObject mapJo;
        private int monsterSpawnCount = 5; // 初始生成的怪物数量
        private int spawnMultiplier = 2; // 每次生成怪物数量的倍数
        private int spawnTimes = 0; // 生成次数

        public RoomStateFight(GameRoom room) : base(room)
        {
            // 初始化静态实例
            _instance = this;
        }

        public override void Enter()
        {
            Broadcast(new Protocol.Msg
            {
                cmd = Protocol.CMD.NtfStartFight,
            });

            gameWorld = new GameWorld();
            roleActorList = new List<RoleActor>();
            cancellationTokenSource = new CancellationTokenSource();
            weaponDic = new Dictionary<long, WeaponActor>();
            monsterActorList = new List<MonsterActor>();
            bulletActorsList = new List<BulletActor>();
            mapJo = JObject.Parse(File.ReadAllText("01Common/Config/AStar.json"));
            SpawnMonsters(monsterSpawnCount);

            for (int i = 0; i < Room.UIdArr.Length; i++)
            {
                RoleActor roleActor = gameWorld.Create<RoleActor>(new Microsoft.Xna.Framework.Vector2(1 + i, 1 + i));
                roleActorList.Add(roleActor);
                roleActor.Id = YitIdHelper.NextId();
                roleActor.RoleState.roleId= CacheSvc.Instance.GetUser(Room.UIdArr[i]).RoleId;
                roleActor.RoleState.uid = Room.UIdArr[i];
                roleActor.RoleState.roleName = CacheSvc.Instance.GetUser(Room.UIdArr[i]).RoleName;
                roleActor.RoleState.pos = roleActor.Body.Position.ToNetVector2();
                roleActor.RoleState.dir = new Protocol.Body.NetVector2();
                roleActor.Start();
            }

            Broadcast(new Protocol.Msg
            {
                cmd = Protocol.CMD.NtfSpawnRole,
                ntfSpawnRole = new Protocol.Body.NtfSpawnRole
                {
                    roleStates = roleActorList.Select(p => p.RoleState).ToArray()
                }
            });

            // 创建武器
            for (int i = 0; i < 2; i++) // 假设生成两个随机武器
            {
                SpawnRandomWeapon();
            }

            Task.Run(FightTask);
        }

        void SpawnMonsters(int count)
        {
            var map = new AStarMap(mapJo);
            for (int i = 0; i < count; i++)
            {
                MonsterActor monsterActor = gameWorld.Create<MonsterActor>(new Microsoft.Xna.Framework.Vector2(2 + i, 2 + i));
                monsterActor.Id = Monsterid + i;
                Monsterid++;
                monsterActor.monsterState.monsterName = "Monster_" + i;
                monsterActor.monsterState.pos = monsterActor.Body.Position.ToNetVector2();
                monsterActor.monsterState.dir = new Protocol.Body.NetVector2();
                monsterActor.Start();
                monsterActorList.Add(monsterActor);
            }

            Broadcast(new Protocol.Msg
            {
                cmd = Protocol.CMD.NtfSpawnMonster,
                ntfSpawnMonster = new Protocol.Body.NtfSpawnMonster
                {
                    monsterStates = monsterActorList.Select(p => p.monsterState).ToArray()
                }
            });
        }
        //产生随机的武器并通知所以客户端
        void SpawnRandomWeapon()
        {
            WeaponObject randomWeaponConfig = ResSvc.Instance.GetRandomWeaponConfig();
            if (randomWeaponConfig != null)
            {
                Random random = new Random();
                float randomX = (float)(random.NextDouble() * 10 - 5); // 随机生成 -5 到 5 之间的浮点数
                float randomY = (float)(random.NextDouble() * 10 - 5); // 随机生成 -5 到 5 之间的浮点数

                WeaponActor weaponActor = gameWorld.Create<WeaponActor>(new Microsoft.Xna.Framework.Vector2(randomX, randomY));
                weaponActor.Id = YitIdHelper.NextId();
                weaponDic.Add(weaponActor.Id, weaponActor);
                weaponActor.UpdateWeapon(randomWeaponConfig);
                weaponActor.Start();

                // 更新武器的坐标
                randomWeaponConfig.pos = new Protocol.Body.NetVector2(randomX, randomY);

                Broadcast(new Protocol.Msg
                {
                    cmd = Protocol.CMD.NtfSpawnWeapon,
                    ntfSpawnWeapon = new NtfSpawnWeapon
                    {
                        weaponObjectArr = new WeaponObject[] { randomWeaponConfig }
                    }
                });
            }
        }

        void FightTask()
        {
          
            int delta = 33;

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                Room.CheckDisconnected();

                gameWorld.Update(delta);
                UpdateMonsters();
                UpdateBullet();
                BroadcastRole();
                BroadcastMonsters();

                Task.Delay(delta).Wait();
            }

            void UpdateMonsters()
            {
                lock (monsterActorList) // 使用锁保护集合访问
                {
                    for (int i = monsterActorList.Count - 1; i >= 0; i--)
                    {
                        var monster = monsterActorList[i];
                       
                        if (monster.hpChanged)
                        {
                            Broadcast(new Protocol.Msg
                            {
                                cmd = Protocol.CMD.NtfMonsterHit,
                                ntfMonsterHit = new Protocol.Body.NtfMonsterHit
                                {
                                    monsterState = monster.monsterState,
                                }
                            });
                            monster.hpChanged = false;
                        }
                        if (monster.isDead())
                        {
                            monsterActorList.Remove(monster);
                            continue;
                        }
                        monster.Update();
                    }
                }

                // 检查是否所有怪物都已被移除
                if (monsterActorList.Count == 0 && spawnTimes < 4)
                {
                    spawnTimes++;
                    monsterSpawnCount *= spawnMultiplier; // 每次生成的怪物数量是上次的两倍
                    SpawnMonsters(monsterSpawnCount);
                }
            }

            void UpdateBullet()
            {
                foreach (var bullet in bulletActorsList.ToList()) // 避免在迭代时修改列表
                {
                    bullet.Update();
                }
                //if (bulletActorsList.Count > 0)
                //{
                //    Broadcast(new Protocol.Msg
                //    {
                //        cmd = Protocol.CMD.NtfBulletState,
                //        ntfBulletState = new Protocol.Body.NtfBulletState
                //        {
                //            BulletStates = bulletActorsList.Select(p => p.BulletState).ToArray()
                //        }
                //    });
                //}
            }

            void BroadcastMonsters()
            {
                List<MonsterState> monsterStateList = new List<MonsterState>();
                lock (monsterActorList)
                {
                    foreach (var v in monsterActorList)
                    {
                        if (v.isStateChanged)
                        {
                            monsterStateList.Add(v.monsterState);
                            v.isStateChanged = false;
                        }
                    }
                }

                if (monsterStateList.Count > 0)
                {
                    Broadcast(new Protocol.Msg
                    {
                        cmd = Protocol.CMD.NtfSyncMonster,
                        ntfSyncMonster = new Protocol.Body.NtfSyncMonster
                        {
                            monsterStates = monsterStateList.ToArray()
                        }
                    });
                }
            }

            void BroadcastRole()
            {
                List<RoleState> roleStateList = new List<RoleState>();
                foreach (var v in roleActorList)
                {
                    if (v.isStateChanged)
                    {
                        roleStateList.Add(v.RoleState);
                        v.isStateChanged = false;
                    }
                }

                if (roleStateList.Count > 0)
                {
                    Broadcast(new Protocol.Msg
                    {
                        cmd = Protocol.CMD.NtfSyncRole,
                        ntfSyncRole = new Protocol.Body.NtfSyncRole
                        {
                            roleStates = roleStateList.ToArray()
                        }
                    });
                }
            }
        }

        // 处理拾取武器请求
        public void HandlePickupWeaponRequest(long uid, long weaponId)
        {
            // 查找对应的玩家和武器
            RoleActor player = roleActorList.FirstOrDefault(p => p.RoleState.uid == uid);
            if (player == null) return;

            WeaponActor weapon;
            if (!weaponDic.TryGetValue(weaponId, out weapon)) return;

            // 角色拾取武器
            player.EquipWeapon(weapon.WeaponObject);

            // 移除武器
            weaponDic.Remove(weaponId);
            gameWorld.Destroy(weapon);

            // 广播武器拾取消息
            Broadcast(new Protocol.Msg
            {
                cmd = Protocol.CMD.RspPickupWeapon,
                rspPickupWeapon = new Protocol.Body.RspPickupWeapon
                {
                    uid = player.RoleState.uid,
                    weaponId = weaponId,
                }
            });
        }

        public void ReqWeaponFire(ReqWeaponFire reqWeaponFire)
        {
            var role = roleActorList.FirstOrDefault(p => p.RoleState.uid == reqWeaponFire.uid);
            if (role != null)
            {
                bool isFireSuccess = false;
                //如果没子弹不允许射击
                //if (role.RoleState.weaponObject.magazinesCount > 0)
                //{
                isFireSuccess = true;

                role.RoleState.weaponObject.magazinesCount--;

                BulletState bulletState = new BulletState()
                {
                    Id = YitIdHelper.NextId(),
                    Speed = 25,
                    StartPos = reqWeaponFire.startPos,
                    EndPos = reqWeaponFire.endPos,
                    BulletName = "bullet1",
                    Pos = reqWeaponFire.startPos,
                    UId = reqWeaponFire.uid
                };
                Vector2 direction = (reqWeaponFire.endPos.ToVector2() - reqWeaponFire.startPos.ToVector2());
                BulletActor bulletActor = gameWorld.Create<BulletActor>(bulletState.Pos.ToVector2());
                bulletActor.Init(bulletState, direction, 5);
                bulletActorsList.Add(bulletActor);

                //}
                Broadcast(new Protocol.Msg
                {
                    cmd = Protocol.CMD.RspWeaponFire,
                    rspWeaponFire = new Protocol.Body.RspWeaponFire
                    {
                        isFireSuccess = isFireSuccess,
                        magCount = role.RoleState.weaponObject.magazinesCount,
                        spareMagCount = role.RoleState.weaponObject.reserveMagazineCount,
                        startPos = reqWeaponFire.startPos,
                        endPos = reqWeaponFire.endPos,
                        bulletState = bulletState,
                    }
                });
                Broadcast(new Protocol.Msg
                {
                    cmd = Protocol.CMD.NtfBulletState,
                    ntfBulletState = new Protocol.Body.NtfBulletState
                    {
                        uid = reqWeaponFire.uid,
                        BulletState = bulletState
                    }
                });
                //this.ColorLog(PEUtils.LogColor.Green, "成功发射");
            }
            else
            {
                this.Warn($"异常id:{reqWeaponFire.uid} 发射子弹！");
            }
        }

        public override void Exit()
        {
            cancellationTokenSource.Cancel();

            // 清理资源
            roleActorList.Clear();
            monsterActorList.Clear();
            bulletActorsList.Clear();
            weaponDic.Clear();

            // 清理其他可能的资源
            gameWorld = null;
            mapJo = null;
        }

        public override void Update()
        {
        }

        public void UpdateRole(int posIndex, RoleState roleState)
        {
            if (posIndex < 0 || posIndex >= roleActorList.Count)
            {
                // 记录日志或处理错误
                Console.WriteLine($"Invalid index: {posIndex}");
                return;
            }
            roleActorList[posIndex].UpdateState(roleState);
        }
        //public void UpdateMonster(int monsterIndex, MonsterState monsterState)
        //{
        //    monsterActorList[monsterIndex].UpdateState(monsterState);
        //}
        public RoleState[] GetRoleState()
        {
            return roleActorList.Select(v => v.RoleState).ToArray();
        }
        public WeaponObject[] GetWeaponObjectArr()
        {
            return weaponDic.Values.Select(p => p.WeaponObject).ToArray();
        }
        public MonsterState[] GetMonsterObjectArr()
        {
            return monsterActorList.Select(v => v.monsterState).ToArray();
        }
        public void RemoveBullet(BulletActor bullet)
        {
            gameWorld.Destroy(bullet);
            bulletActorsList.Remove(bullet);
        }

        public List<MonsterActor> GetMonsters()
        {
            return monsterActorList;
        }
        public List<RoleActor> GetRoleActors()
        {
            return roleActorList;
        }
    }


}
