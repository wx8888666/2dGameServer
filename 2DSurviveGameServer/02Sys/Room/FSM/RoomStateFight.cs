using _2DSurviveGameServer._02Sys.Room.Actors;
using _2DSurviveGameServer._03Svc;
using _2DSurviveGameServer.Helpers;
using GameEngine;
using Protocol.Body;

namespace _2DSurviveGameServer._02Sys.Room.FSM
{
    public class RoomStateFight : RoomStateBase
    {
        GameWorld gameWorld;
        List<RoleActor> roleActorList;
        CancellationTokenSource cancellationTokenSource;

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

            for(int i = 0;i<Room.UIdArr.Length;i++)
            {
                RoleActor roleActor = gameWorld.Create<RoleActor>(new Microsoft.Xna.Framework.Vector2(1 + i, 1 + i));
                roleActorList.Add(roleActor);
                roleActor.RoleState.uid = Room.UIdArr[i];
                roleActor.RoleState.roleName = CacheSvc.Instance.GetUser(Room.UIdArr[i]).RoleName;
                roleActor.RoleState.pos = roleActor.Body.Position.ToNetVector2();
                roleActor.RoleState.dir = new Protocol.Body.NetVector2();
            }


            Broadcast(new Protocol.Msg
            {
                cmd = Protocol.CMD.NtfSpawnRole,
                ntfSpawnRole = new Protocol.Body.NtfSpawnRole
                {
                    roleStates = roleActorList.Select(p=>p.RoleState).ToArray()
                }
            });

            Task.Run(FightTask);
        }

        void FightTask()
        {
            int delta = 33;
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
        //public WeaponObject[] GetWeaponObjectArr()
        //{
        //    re
        //}
    }
}
