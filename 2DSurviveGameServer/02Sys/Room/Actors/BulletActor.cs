using _2DSurviveGameServer._02Sys.Room.FSM;
using _2DSurviveGameServer.Helpers;
using FarseerPhysics.Dynamics;
using GameEngine;
using Microsoft.Xna.Framework;
using Protocol.Body;

namespace _2DSurviveGameServer._02Sys.Room.Actors
{
    public class BulletActor : Actor
    {
        public BulletState BulletState { get; private set; }
        private Vector2 direction;
        private int damage;
        private float destroyTimeCounter = 0f;
        private float destroyTime = 2f; 

        public void Init(BulletState bulletState, Vector2 direction, int damage)
        {
            this.BulletState = bulletState;
            this.direction = direction;
            this.damage = damage;
       
            Body.Position = BulletState.Pos.ToVector2();
        }

        public override void Start()
        {
        }

        public override void Update()
        {
            float deltaTime = 1 / 33f;
            Vector2 newPos = BulletState.Pos.ToVector2() + direction * BulletState.Speed * deltaTime;
            BulletState.Pos = newPos.ToNetVector2(); // 如果 BulletState.Pos 是某种向量类型，请确保它有合适的转换方法

            Body.Position = newPos; // 将新位置赋值给物理引擎中的 Body 对象

            if (IsColliding())
            {
                HandleCollision();
                //this.Log("击中");
                return;
            }

            destroyTimeCounter += (int)(deltaTime * 1000);
            if (destroyTimeCounter > destroyTime)
            {
                OnDestory();
            }
        }

        public override void OnDestory()
        {
          
            // 从子弹列表中移除子弹
            RoomStateFight.Instance.RemoveBullet(this);
        }

        private void HandleCollision()
        {
            OnDestory();
        }

        private bool IsColliding()
        {
            // 检测与敌人的碰撞逻辑
            foreach (var enemy in RoomStateFight.Instance.GetMonsters())
            {
                if (Vector2.Distance(Body.Position, enemy.Body.Position) < 5.0f) // 假设碰撞距离为1.0f
                {
                    // 对敌人造成伤害
                    enemy.TakeDamage(damage,BulletState.UId);
                    //this.Log(enemy.Id);
                    return true;
                }
            }
            return false;
        }

    }
}
