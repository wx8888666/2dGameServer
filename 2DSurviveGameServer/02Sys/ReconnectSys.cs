using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer._02Sys.Room;
using _2DSurviveGameServer._02Sys.Room.FSM;
using _2DSurviveGameServer._03Svc;
using Protocol.Body;
using Protocol.DBModel;

namespace _2DSurviveGameServer._02Sys
{
    public class ReconnectSys : SysRoot<ReconnectSys>
    {
        public override void Init()
        {
            base.Init();

            netSvc.AddMsgHandle(Protocol.CMD.ReqClientState, ReqClientState);
        }

        void ReqClientState(MsgPack pack)
        {
            ReqClientState req = pack.msg.reqClientState;
            ServerSession session = pack.session;
            long uid = req.uid;

            if (uid == 0) return;

            ClientStateEnum clientState = cacheSvc.GetClientState(uid);
            //处于大厅
            if(clientState == ClientStateEnum.None)
            {
                session.SendMsg(new Protocol.Msg
                {
                    cmd = Protocol.CMD.RspClientState,
                    rspClientState = new RspClientState
                    {
                        clientState = ClientStateEnum.None,
                    }
                });
            }
            //处于匹配队列中
            if(clientState == ClientStateEnum.MatchingQueue)
            {
                session.SendMsg(new Protocol.Msg
                {
                    cmd = Protocol.CMD.RspClientState,
                    rspClientState = new RspClientState
                    {
                        clientState = ClientStateEnum.MatchingQueue,
                    }
                });
            }
            //处于房间确认
            if(clientState == ClientStateEnum.RoomConfirm)
            {
                GameRoom room = RoomSys.Instance.GetGameRoomByUid(uid);
                if(room != null)
                {
                    List<User> userList = new List<User>();
                    for(int i = 0;i<room.UIdArr.Length;i++)
                    {
                        userList.Add(cacheSvc.GetUser(room.UIdArr[i]));
                    }

                    RoomStateConfirm roomStateConfirm = (RoomStateConfirm)room.GetRoomState(Room.FSM.RoomStateEnum.Confirm);

                    session.SendMsg(new Protocol.Msg
                    {
                        cmd = Protocol.CMD.RspClientState,
                        rspClientState = new RspClientState
                        {
                            clientState = ClientStateEnum.RoomConfirm,
                            roomConfirmState = new RoomConfirmState
                            {
                                roomId = room.RoomId,
                                userArr = userList.ToArray(),
                                confirmArr = roomStateConfirm.confirmArr,
                            }
                        }
                    });
                }
            }
            //处于房间加载
            if(clientState == ClientStateEnum.RoomLoad)
            {
                GameRoom room = RoomSys.Instance.GetGameRoomByUid(uid);
                if(room != null)
                {
                    List<User> userList = new List<User>();
                    for (int i = 0; i < room.UIdArr.Length; i++)
                    {
                        userList.Add(cacheSvc.GetUser(room.UIdArr[i]));
                    }

                    RoomStateLoad roomStateLoad = (RoomStateLoad)room.GetRoomState(Room.FSM.RoomStateEnum.Load);

                    session.SendMsg(new Protocol.Msg
                    {
                        cmd = Protocol.CMD.RspClientState,
                        rspClientState = new RspClientState
                        {
                            clientState = ClientStateEnum.RoomLoad,
                            roomLoadState = new RoomLoadState
                            {
                                roomId = room.RoomId,
                                userArr = userList.ToArray(),
                                progressArr = roomStateLoad.progressArr,
                            }
                        }
                    });
                }
            }
            //处于战斗状态
            if(clientState == ClientStateEnum.RoomFight)
            {
                GameRoom room = RoomSys.Instance.GetGameRoomByUid(uid);
                if (room != null)
                {
                    List<User> userList = new List<User>();
                    for (int i = 0; i < room.UIdArr.Length; i++)
                    {
                        userList.Add(cacheSvc.GetUser(room.UIdArr[i]));
                    }

                    RoomStateFight roomStateFight = (RoomStateFight)room.GetRoomState(Room.FSM.RoomStateEnum.Fight);

                    session.SendMsg(new Protocol.Msg
                    {
                        cmd = Protocol.CMD.RspClientState,
                        rspClientState = new RspClientState
                        {
                            clientState = ClientStateEnum.RoomFight,
                            roomFightState = new RoomFightState
                            {
                                roomId = room.RoomId,
                                userArr = userList.ToArray(),
                                roleStateArr = roomStateFight.GetRoleState(),
                                weaponObjectArr=roomStateFight.GetWeaponObjectArr(),
                            }
                        }
                    }) ; 
                }
            }
        }
    }
}
