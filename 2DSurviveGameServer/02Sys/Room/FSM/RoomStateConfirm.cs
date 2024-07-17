using _2DSurviveGameServer._03Svc;
using System.Drawing;

namespace _2DSurviveGameServer._02Sys.Room.FSM
{
    public class RoomStateConfirm : RoomStateBase
    {
        public bool[] confirmArr;
        int checkTaskId;

        public RoomStateConfirm(GameRoom room) : base(room)
        {
        }

        public override void Enter()
        {
            confirmArr = new bool[Room.UIdArr.Length];
            TimerSvc.Instance.AddTask(10000, ReachTimeLimit);
        }

        public override void Exit()
        {
        }

        public override void Update()
        {
        }

        void ReachTimeLimit()
        {
            if (CheckConfirmState()) return;

            Broadcast(new Protocol.Msg
            {
                cmd = Protocol.CMD.NtfMatchDisband,
            });

            this.Log($"id为{Room.RoomId}的房间因为准备确认倒计时结束没有完成准备，所以房间提前解散");
            ChangeRoomState(RoomStateEnum.End);
        }
        

        bool CheckConfirmState()
        {
            for(int i = 0; i < confirmArr.Length; i++)
            {
                if(confirmArr[i] == false)
                {
                    return false;
                }
            }
            return true;
        }

        public void UpdateConfirmState(int posIndex)
        {
            confirmArr[posIndex] = true;

            Broadcast(new Protocol.Msg
            {
                cmd = Protocol.CMD.NtfConfirm,
                ntfConfirm = new Protocol.Body.NtfConfirm
                {
                    confirmArr = confirmArr
                }
            });

            if (CheckConfirmState())
            {
                TimerSvc.Instance.DeleteTask(checkTaskId);
                ChangeRoomState(RoomStateEnum.Load);
            }
        }
    }
}
