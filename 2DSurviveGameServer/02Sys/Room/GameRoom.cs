using _2DSurviveGameServer._02Sys.Room.FSM;
using _2DSurviveGameServer._03Svc;
using PENet;
using Protocol;
using Protocol.Body;
using System.Xml.Serialization;

namespace _2DSurviveGameServer._02Sys.Room
{
    public class GameRoom
    {
        public long RoomId { get; private set; }//当前房间ID
        public long[] UIdArr { get;private set; }//当前房间内所有玩家的UID
        public bool[] OnlineArr { get; private set; }//当前房间内在线的玩家
        public bool[] ExitArr { get; private set; }//当前房间退出的玩家

        Dictionary<RoomStateEnum, RoomStateBase> fsm;
        RoomStateEnum currentRoomState;

        public GameRoom(long roomId, long[] uidArr)
        {
            RoomId = roomId;
            UIdArr = uidArr;
            OnlineArr = new bool[uidArr.Length];
            for(int i = 0; i < OnlineArr.Length; i++)
            {
                OnlineArr[i] = true;
            }
            ExitArr = new bool[uidArr.Length];
            fsm = new Dictionary<RoomStateEnum, RoomStateBase>();
            currentRoomState = RoomStateEnum.None;

            fsm.Add(RoomStateEnum.Confirm, new RoomStateConfirm(this));
            fsm.Add(RoomStateEnum.Load,new RoomStateLoad(this));
            fsm.Add(RoomStateEnum.Fight,new RoomStateFight(this));
            fsm.Add(RoomStateEnum.End,new RoomStateEnd(this));

            ChangeRoomState(RoomStateEnum.Confirm);

            Msg msg = new Msg()
            {
                cmd = CMD.NtfMatchComplete,
                ntfMatchComplete = new Protocol.Body.NtfMatchComplete
                {
                    roomId = roomId,
                    userArr = new Protocol.DBModel.User[uidArr.Length]
                }
            };

            for(int i = 0;i < uidArr.Length;i++)
            {
                msg.ntfMatchComplete.userArr[i] = CacheSvc.Instance.GetUser(uidArr[i]);
            }

            Broadcast(msg);
        }

        public void ChangeRoomState(RoomStateEnum targetState)
        {
            if (currentRoomState == targetState) return;

            if (fsm.ContainsKey(targetState))
            {
                if(currentRoomState != RoomStateEnum.None)
                {
                    fsm[currentRoomState].Exit();
                }
                fsm[targetState].Enter();
                currentRoomState = targetState;
                for(int i = 0;i<UIdArr.Length;i++)
                {
                    if (targetState == RoomStateEnum.Confirm)
                    {
                        //改变当前客户端的状态：房间确认
                        CacheSvc.Instance.UpdateClientState(UIdArr[i], ClientStateEnum.RoomConfirm);
                    }
                    if (targetState == RoomStateEnum.Load)
                    {
                        //改变当前客户端的状态：房间加载
                        CacheSvc.Instance.UpdateClientState(UIdArr[i], ClientStateEnum.RoomLoad);
                    }
                    if (targetState == RoomStateEnum.Fight)
                    {
                        //改变当前客户端的状态：房间战斗
                        CacheSvc.Instance.UpdateClientState(UIdArr[i], ClientStateEnum.RoomFight);
                    }
                    if (targetState == RoomStateEnum.End)
                    {
                        //改变当前客户端的状态：None（战斗结束后把房间内所有玩家状态设置为None无状态）
                        CacheSvc.Instance.UpdateClientState(UIdArr[i], ClientStateEnum.None);
                    }
                }
                this.Log($"id为{RoomId}的房间现在进入{targetState}状态");
            }
        }

        public RoomStateBase GetRoomState(RoomStateEnum roomState)
        {
            return fsm[roomState];
        }

        public void CheckDisconnected()
        {
            for (int i = 0; i < UIdArr.Length; i++)
            {
                if (!CacheSvc.Instance.IsOnline(UIdArr[i]))
                {
                    OnlineArr[i] = false;
                }
            }
        }

        public void Broadcast(Msg msg)
        {
            CheckDisconnected();
            byte[] bytes = KCPTool.Serialize(msg);
            if(bytes != null)
            {
                for(int i = 0;i<UIdArr.Length;i++)
                {
                    if (OnlineArr[i])
                    {
                        CacheSvc.Instance.GetSession(UIdArr[i])?.SendMsg(bytes);
                    }
                }
            }
        }

        public void SendTo(Msg msg,int posIndex)
        {
            CheckDisconnected();
            if (OnlineArr[posIndex])
            {
                CacheSvc.Instance.GetSession(UIdArr[posIndex])?.SendMsg(msg);
            }
        }

        public void SendExcept(Msg msg,int posIndex)
        {
            CheckDisconnected();
            byte[] bytes = KCPTool.Serialize(msg);
            for (int i = 0; i < UIdArr.Length; i++)
            {
                if (i == posIndex) continue;

                if (OnlineArr[i])
                {
                    CacheSvc.Instance.GetSession(UIdArr[i])?.SendMsg(bytes);
                }
            }
        }

        public int GetPosIndex(long uid)
        {
            for (int i = 0; i < UIdArr.Length; i++)
            {
                if (UIdArr[i] == uid)
                {
                    return i;
                }
            }

            return -1;
        }


        public void SndConfirm(long uid)
        {
            if(currentRoomState == RoomStateEnum.Confirm)
            {
                if (fsm[currentRoomState] is RoomStateConfirm state)
                {
                    state.UpdateConfirmState(GetPosIndex(uid));
                }
            }
        }

        public void SndLoadPrg(long uid,int prg)
        {
            if(currentRoomState == RoomStateEnum.Load)
            {
                if (fsm[currentRoomState] is RoomStateLoad state)
                {
                    state.UpdateLoadPrg(GetPosIndex(uid), prg);
                }
            }
        }

        public void SndRoleState(long uid,RoleState roleState)
        {
            if (currentRoomState == RoomStateEnum.Fight)
            {
                if (fsm[currentRoomState] is RoomStateFight state)
                {
                    state.UpdateRole(GetPosIndex(uid), roleState);
                }
            }
        }

        public void SndEnterRoom(long uid)
        {
            OnlineArr[GetPosIndex(uid)]=true;
        }
        public void ReqPickupWeapon(ReqPickupWeapon reqPickupWeapon)
        {
            if (currentRoomState == RoomStateEnum.Fight)
            {
                if (fsm[currentRoomState] is RoomStateFight state)
                {
                    state.HandlePickupWeaponRequest(reqPickupWeapon.uid, reqPickupWeapon.weaponId);
                }
            }
        }
    }
}
