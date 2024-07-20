using _2DSurviveGameServer.Helpers;
using GameEngine;
using Protocol.Body;

namespace _2DSurviveGameServer._02Sys.Room.Actors
{
    public class RoleActor : Actor
    {
        //为什么有RoleState这个类呢？是因为服务端与客户端的类不是一样的，要把服务端
        //的一个类放到客户端上，那么就可以使用Rolestate来充当客户端上的。从而达到同步的状态
        public RoleState RoleState { get; set; } = new RoleState();
        public bool isStateChanged { get; set; } = false;
        private readonly object bodyLock = new object(); // 锁对象，用于同步对 Body 的访问
        public override void OnDestory()
        {
        }
        public  void EquipWeapon(WeaponObject weaponObject)
        { 
            RoleState.weaponObject = weaponObject;
        }
        public override void Start()
        {
            
            RoleState.id=this.Id;
            RoleState.hp = 20;
            RoleState.maxHp=20;
        }

        public override void Update()
        {

        }
        public void getIntegral()
        {
            RoleState.integral += 5;
            this.Log(RoleState.integral);
            isStateChanged = true;
        }
        public void UpdateState(RoleState roleState)
        {
            if (roleState.id < 0)
            {
                // 记录日志或处理错误
                Console.WriteLine($"Invalid proxy ID: {roleState.id}");
                return;
            }
            lock (bodyLock) // 锁定以确保对 Body 的线程安全访问
            {
                RoleState.pos = roleState.pos;
                RoleState.dir = roleState.dir;
                Body.Position = roleState.pos.ToVector2();
            }

            RoleState.mousePos = roleState.mousePos;
            isStateChanged = true;
        }
    }
}
