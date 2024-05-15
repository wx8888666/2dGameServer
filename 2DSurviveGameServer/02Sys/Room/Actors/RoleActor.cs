using _2DSurviveGameServer.Helpers;
using GameEngine;
using Protocol.Body;

namespace _2DSurviveGameServer._02Sys.Room.Actors
{
    public class RoleActor : Actor
    {
        public RoleState RoleState { get; set; } = new RoleState();
        public bool isStateChanged { get; set; } = false;

        public override void OnDestory()
        {
        }

        public override void Start()
        {
        }

        public override void Update()
        {
        }

        public void UpdateState(RoleState roleState)
        {
            RoleState.pos = roleState.pos;
            RoleState.dir = roleState.dir;
            Body.Position = roleState.pos.ToVector2();
            isStateChanged = true;
        }
    }
}
