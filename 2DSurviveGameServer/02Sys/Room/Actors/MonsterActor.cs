using _2DSurviveGameServer.Helpers;
using FarseerPhysics.Dynamics;
using GameEngine;
using Microsoft.Xna.Framework;
using Protocol.Body;

namespace _2DSurviveGameServer._02Sys.Room.Actors
{
    using System;
    using System.Diagnostics;

    public class MonsterActor : Actor
    {
        public MonsterState MonsterState { get; set; } = new MonsterState();
        public bool isStateChanged { get; set; } = false;
        private Vector2 targetPosition;
        private Random random = new Random();
        private float moveSpeed = 0.1f; // 添加移动速度变量
        private float moveDelay = 5f; // 添加较长的延迟移动时间，单位为秒

        private Stopwatch stopwatch = new Stopwatch();

        public override void OnDestory()
        {
            // 停止计时器
            stopwatch.Stop();
        }

        public override void Start()
        {
            MonsterState.id = this.Id;
            SetRandomTargetPosition();

            // 启动计时器
            stopwatch.Start();
        }

        public override void Update()
        {
            // 获取经过的时间
            TimeSpan elapsedTime = stopwatch.Elapsed;

            // 如果经过的时间超过移动延迟，则重新设置目标位置
            if (elapsedTime.TotalSeconds >= moveDelay)
            {
                SetRandomTargetPosition();

                // 重置计时器
                stopwatch.Restart();
            }

            MoveToTargetPosition();
        }

        private void SetRandomTargetPosition()
        {
            float x = (float)(random.NextDouble() * 10); 
            float y = (float)(random.NextDouble() * 10);
            targetPosition = new Vector2(x, y);
        }

        private void MoveToTargetPosition()
        {
            Vector2 direction = Vector2.Normalize(targetPosition - Body.Position);
            Body.Position += direction * moveSpeed;

            MonsterState.pos = Body.Position.ToNetVector2();
            MonsterState.dir = direction.ToNetVector2();
            isStateChanged = true;
        }

        public void UpdateState(MonsterState monsterState)
        {
            MonsterState.pos = monsterState.pos;
            MonsterState.dir = monsterState.dir;
            Body.Position = monsterState.pos.ToVector2();
            isStateChanged = true;
        }

        public Vector2 AStarPathfinding(Vector2 start, Vector2 target)
        {
            // Here you would implement your A* algorithm to find the path from start to target
            // For demonstration purposes, we'll just return the target position directly
            return target;
        }
    }


}
