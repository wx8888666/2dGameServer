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
        private int destroyTimeCounter = 0;
        private int destroyTime = 5000;

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
            // 这里可以替换为实际的deltaTime，例如通过Time.deltaTime
            float deltaTime = 1 / 60f; // 假设帧率是60 FPS
            Vector2 newPos = BulletState.Pos.ToVector2() + direction * BulletState.Speed * deltaTime;
            BulletState.Pos = newPos.ToNetVector2(); // 如果 BulletState.Pos 是某种向量类型，请确保它有合适的转换方法

            Body.Position = newPos; // 将新位置赋值给物理引擎中的 Body 对象

            if (IsColliding())
            {
                HandleCollision();
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
           
            
           
        }

        private void HandleCollision()
        {
            OnDestory();
        }
    }
}
