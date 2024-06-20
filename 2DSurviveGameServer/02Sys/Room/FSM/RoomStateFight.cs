using _2DSurviveGameServer._02Sys.Room.Actors;
using _2DSurviveGameServer._03Svc;
using _2DSurviveGameServer.Helpers;
using GameEngine;
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

            for(int i = 0;i<Room.UIdArr.Length;i++)
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
            int delta = 50;
            while(!cancellationTokenSource.IsCancellationRequested)
            {
                Room.CheckDisconnected();

                gameWorld.Update(delta);

                BroadcastRole();

                Task.Delay(delta).Wait();
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
        public  RoleState[] GetRoleState()
        {
            return roleActorList.Select(v => v.RoleState).ToArray();
        }
        public WeaponObject[] GetWeaponObjectArr()
        {
            return weaponDic.Values.Select( p =>p.WeaponObject ).ToArray();
        }
    }
}
