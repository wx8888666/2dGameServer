using _2DSurviveGameServer.Helpers;
using FarseerPhysics.Dynamics;
using GameEngine;
using Microsoft.Xna.Framework;
using Protocol.Body;

namespace _2DSurviveGameServer._02Sys.Room.Actors
{
    using _2DSurviveGameServer._01Common;
    using System;
    using System.Diagnostics;

    public class MonsterActor : Actor
    {
        public MonsterState monsterState { get; set; } = new MonsterState();
        public bool isStateChanged { get; set; } = false;
        private Vector2 targetPosition;
        private Random random = new Random();
        private float moveSpeed = 0.1f; // 添加移动速度变量
        private float moveDelay = 5f; // 添加较长的延迟移动时间，单位为秒

        private Stopwatch stopwatch = new Stopwatch();
        private AStarPathfinder pathfinder;
        public bool hpChanged=false;
        // 无参数构造函数
        public MonsterActor()
        {
        }

        public void Initialize(GameWorld gameWorld)
        {
            // 假设地图大小为 100x100
            pathfinder = new AStarPathfinder(50, 50);
        }

        public override void OnDestory()
        {
            // 停止计时器
            stopwatch.Stop();
        }

        public override void Start()
        {
            monsterState.id = this.Id;
            monsterState.Maxhp = 20;
            monsterState.hp = 20;
            monsterState.state = StateEnum.Alive;
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

            monsterState.pos = Body.Position.ToNetVector2();
            monsterState.dir = direction.ToNetVector2();
            isStateChanged = true;
        }

        public void UpdateState(MonsterState monsterState)
        {
            monsterState.pos = monsterState.pos;
            monsterState.dir = monsterState.dir;
            Body.Position = monsterState.pos.ToVector2();
            isStateChanged = true;
        }

        public Vector2 AStarPathfinding(Vector2 start, Vector2 target)
        {
            var path = pathfinder.FindPath(start, target);
            if (path.Count > 0)
            {
                return path[0]; // 返回路径的第一个节点作为下一个目标位置
            }
            return target;
        }
        public void MoveToTargetPosition(Vector2 targetPosition)
        {
            Vector2 direction = Vector2.Normalize(targetPosition - Body.Position);
            Body.Position += direction * moveSpeed;

            monsterState.pos = Body.Position.ToNetVector2();
            monsterState.dir = direction.ToNetVector2();
            isStateChanged = true;
        }

        public void TakeDamage(int damage)
        {
            if (monsterState.hp == 0)
            {
                return;
            }
            monsterState.hp -= damage;
            hpChanged = true;
            if (monsterState.hp <= 0)
            {
                monsterState.hp = 0;
            }
        }
        public bool isDead()
        {
            if(monsterState.hp==0)return true;
            return false;   
        }
        
    }

}
