namespace _2DSurviveGameServer._02Sys.Room.FSM
{
    public class RoomStateEnd : RoomStateBase
    {
        public RoomStateEnd(GameRoom room) : base(room)
        {
        }

        public override void Enter()
        {
            RoomSys.Instance.Remove(Room.RoomId);
        }

        public override void Exit()
        {
        }

        public override void Update()
        {
        }
    }
}
