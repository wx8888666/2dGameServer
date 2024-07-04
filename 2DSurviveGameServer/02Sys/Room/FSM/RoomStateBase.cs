using Protocol;

namespace _2DSurviveGameServer._02Sys.Room.FSM
{
    public interface IRoomState
    {
        void Enter();
        void Update();
        void Exit();
    }

    public enum RoomStateEnum
    {
        None,
        Confirm,
        Load,
        Fight,
        End,
    }

    public abstract class RoomStateBase : IRoomState
    {
        protected GameRoom Room { get;private set; }

        protected RoomStateBase(GameRoom room)
        {
            Room = room;
        }

        protected void Broadcast(Msg msg)
        {
            Room.Broadcast(msg);
        }

        protected void SendExcept(Msg msg, int posIndex)
        {
            Room.SendExcept(msg, posIndex);
        }
        protected void SendTo(Msg msg, int posIndex)
        {
            Room.SendTo(msg, posIndex);
        }
        protected void ChangeRoomState(RoomStateEnum roomStateEnum)
        {
            Room.ChangeRoomState(roomStateEnum);
        }

        public abstract void Enter();

        public abstract void Exit();

        public abstract void Update();
    }
}
