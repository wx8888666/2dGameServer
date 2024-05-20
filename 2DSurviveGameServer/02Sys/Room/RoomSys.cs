using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer._03Svc;
using Protocol;
using Protocol.Body;
using Yitter.IdGenerator;

namespace _2DSurviveGameServer._02Sys.Room
{
    public class RoomSys:SysRoot<RoomSys>
    {
        Dictionary<long, GameRoom> gameRoomDic;

        public override void Init()
        {
            base.Init();
            gameRoomDic = new Dictionary<long, GameRoom>();

            netSvc.AddMsgHandle(CMD.SndConfirm, SndConfirm);
            netSvc.AddMsgHandle(CMD.SndLoadPrg, SndLoadPrg);
            netSvc.AddMsgHandle(CMD.SndRoleState, SndRoleState);
            netSvc.AddMsgHandle(CMD.SndEnterRoom, SndEnterRoom);
        }

        public override void Update()
        {
            base.Update();
        }

        public void Add(long[] uidArr)
        {
            long roomId = YitIdHelper.NextId();
            GameRoom room = new GameRoom(roomId, uidArr);
            gameRoomDic.Add(roomId, room);
            this.Log($"创建了一个房间，当前房间数量：{gameRoomDic.Count}");
        }

        public void Remove(long roomId)
        {
            gameRoomDic.Remove(roomId);
            GC.Collect();
            this.Log($"销毁了一个房间，当前房间数量：{gameRoomDic.Count}");
        }


        public GameRoom GetGameRoomByUid(long uid)
        {
            foreach(var room in gameRoomDic.Values)
            {
                foreach(var id in room.UIdArr)
                {
                    if(id == uid) return room;
                }
            }

            return null;
        }

        void SndConfirm(MsgPack pack)
        {
            if(gameRoomDic.TryGetValue(pack.msg.sndConfirm.roomId,out GameRoom room))
            {
                room.SndConfirm(pack.msg.sndConfirm.uid);
            }
        }

        void SndLoadPrg(MsgPack pack)
        {
            if(gameRoomDic.TryGetValue(pack.msg.sndLoadPrg.roomId,out GameRoom room))
            {
                room.SndLoadPrg(pack.msg.sndLoadPrg.uid, pack.msg.sndLoadPrg.progress);
            }
        }

        void SndRoleState(MsgPack pack)
        {
            if (gameRoomDic.TryGetValue(pack.msg.sndRoleState.roomId, out GameRoom room))
            {
                room.SndRoleState(pack.msg.sndRoleState.uid, pack.msg.sndRoleState.roleState);
            }
        }
        void SndEnterRoom(MsgPack pack)
        {
            SndEnterRoom snd=pack.msg.sndEnterRoom;
            if (gameRoomDic.TryGetValue(snd.roomId, out GameRoom room))
            {
                room.SndEnterRoom(snd.uid);
            }

        }
    }
}
