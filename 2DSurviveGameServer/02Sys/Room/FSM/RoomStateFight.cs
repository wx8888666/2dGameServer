﻿using _2DSurviveGameServer._02Sys.Room.Actors;
using _2DSurviveGameServer._03Svc;
using _2DSurviveGameServer.Helpers;
using GameEngine;
using Microsoft.Xna.Framework;
using Protocol.Body;
using Yitter.IdGenerator;

namespace _2DSurviveGameServer._02Sys.Room.FSM
{
    public class RoomStateFight : RoomStateBase
    {
        GameWorld gameWorld;
        List<RoleActor> roleActorList;
        CancellationTokenSource cancellationTokenSource;
        Dictionary<long, WeaponActor> weaponDic;
        List<MonsterActor> monsterActorList;
        static int Monsterid = 1;
        public RoomStateFight(GameRoom room) : base(room)
        {
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
            SpawnMonsters(5);

            for (int i = 0;i<Room.UIdArr.Length;i++)
            {
                RoleActor roleActor = gameWorld.Create<RoleActor>(new Microsoft.Xna.Framework.Vector2(1 + i, 1 + i));
                roleActorList.Add(roleActor); 
                roleActor.Id=YitIdHelper.NextId();
                roleActor.RoleState.uid = Room.UIdArr[i];
                roleActor.RoleState.roleName = CacheSvc.Instance.GetUser(Room.UIdArr[i]).RoleName;
                roleActor.RoleState.pos = roleActor.Body.Position.ToNetVector2();
                roleActor.RoleState.dir = new Protocol.Body.NetVector2();
                //通过这个方法来赋值roleActor.RoleState.id
                roleActor.Start();
            }


            Broadcast(new Protocol.Msg
            {
                cmd = Protocol.CMD.NtfSpawnRole,
                ntfSpawnRole = new Protocol.Body.NtfSpawnRole
                {
                    roleStates = roleActorList.Select(p=>p.RoleState).ToArray()
                }
            });
            //创建武器
            SpawnWeapon(new WeaponObject[]{ 
                new WeaponObject{ 
                assetId=0,
                pos= new Protocol.Body.NetVector2(0, 0),
                },
               new WeaponObject{
                assetId=1,
                pos= new Protocol.Body.NetVector2(1, 3),
                }
            });

            Task.Run(FightTask);
        }
        void SpawnMonsters(int count)
        {
            for (int i = 0; i < count; i++)
            {
                MonsterActor monsterActor = gameWorld.Create<MonsterActor>(new Microsoft.Xna.Framework.Vector2(2 + i, 2 + i));
                monsterActor.Id = Monsterid + i;
                Monsterid ++;
                monsterActor.MonsterState.monsterName = "Monster_" + i;
                monsterActor.MonsterState.pos = monsterActor.Body.Position.ToNetVector2();
                monsterActor.MonsterState.dir = new Protocol.Body.NetVector2();
                monsterActor.Start();
                monsterActorList.Add(monsterActor);
            }

            Broadcast(new Protocol.Msg
            {
                cmd = Protocol.CMD.NtfSpawnMonster,
                ntfSpawnMonster = new Protocol.Body.NtfSpawnMonster
                {
                    monsterStates = monsterActorList.Select(p => p.MonsterState).ToArray()
                }
            });
        }
        void SpawnWeapon(params WeaponObject[] weaponObject)
        {
            foreach (WeaponObject weapon in weaponObject)
            {
                WeaponActor weaponActor = gameWorld.Create<WeaponActor>(new Microsoft.Xna.Framework.Vector2(3,3));
                weaponActor.Id=YitIdHelper.NextId();
                weaponDic.Add(weaponActor.Id, weaponActor);
                weaponActor.UpdateWeapon(weapon);
                weaponActor.Start();
            }
            Broadcast(new Protocol.Msg
            {

                cmd = Protocol.CMD.NtfSpawnWeapon,
                ntfSpawnWeapon = new NtfSpawnWeapon
                {
                    weaponObjectArr = weaponObject
                }
            });
        }
        void FightTask()
        {
            int delta = 33;
            
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                Room.CheckDisconnected();

                gameWorld.Update(delta);
                UpdateMonsters();
                BroadcastRole();
                BroadcastMonsters();

                Task.Delay(delta).Wait();
            }
            void UpdateMonsters()
            {
                foreach (var monster in monsterActorList)
                {
                   
                    foreach (var player in roleActorList)
                    {
                        if (Vector2.Distance(monster.Body.Position, player.Body.Position) < 5.0f) // Assuming 5.0 is the chase range
                        {
                            //Vector2 pathToPlayer = monster.AStarPathfinding(monster.Body.Position, player.Body.Position);
                            //monster.MoveToTargetPosition();
                            //monster.isStateChanged = true;
                            //这里通过A*算法来实现
                            break;
                        }
                    }
                    monster.Update();
                }
            }
            void BroadcastMonsters()
            {
                List<MonsterState> monsterStateList = new List<MonsterState>();
                foreach (var v in monsterActorList)
                {
                    if (v.isStateChanged)
                    {
                        monsterStateList.Add(v.MonsterState);
                        v.isStateChanged = false;
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
                foreach(var v in roleActorList)
                {
                    if (v.isStateChanged)
                    {
                        roleStateList.Add(v.RoleState);
                        v.isStateChanged = false;
                    }
                }

                if(roleStateList.Count > 0)
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
            }) ;
        }



        public override void Exit()
        {
            cancellationTokenSource.Cancel();
        }

        public override void Update()
        {
        }

    
        public void UpdateRole(int posIndex,RoleState roleState)
        {
            roleActorList[posIndex].UpdateState(roleState);
        }
        public void UpdateMonster(int monsterIndex, MonsterState monsterState)
        {
            monsterActorList[monsterIndex].UpdateState(monsterState);
        }
        public  RoleState[] GetRoleState()
        {
            return roleActorList.Select(v => v.RoleState).ToArray();
        }
        public WeaponObject[] GetWeaponObjectArr()
        {
            return weaponDic.Values.Select( p =>p.WeaponObject ).ToArray();
        }
        public MonsterState[] GetMonsterObjectArr()
        {
            return monsterActorList.Select(v=>v.MonsterState).ToArray();
        }
    }
}
