namespace _2DSurviveGameServer._02Sys.Room.FSM
{
    public class RoomStateLoad : RoomStateBase
    {
        public int[] progressArr;

        public RoomStateLoad(GameRoom room) : base(room)
        {
        }

        public override void Enter()
        {
            progressArr = new int[Room.UIdArr.Length];

            //通知客户端进入加载状态
            Broadcast(new Protocol.Msg
            {
                cmd = Protocol.CMD.NtfStartLoad
            });
        }

        public override void Exit()
        {
        }

        public override void Update()
        {
        }

        bool CheckPrg()
        {
            for(int i = 0; i < progressArr.Length; i++)
            {
                if (progressArr[i] < 100)
                {
                    return false;
                }
            }
            return true;
        }

        public void UpdateLoadPrg(int posIndex,int progress)
        {
            progressArr[posIndex] = progress;

            Broadcast(new Protocol.Msg
            {
                cmd = Protocol.CMD.NtfLoadPrg,
                ntfLoadPrg = new Protocol.Body.NtfLoadPrg
                {
                    progressArr = progressArr,
                }
            });

            if (CheckPrg())
            {
                //全部都已经加载完成了
                ChangeRoomState(RoomStateEnum.Fight);
            }
        }
    }
}
