using _2DSurviveGameServer.Helpers;
using FarseerPhysics.Dynamics;
using GameEngine;
using Microsoft.Xna.Framework;
using Protocol.Body;

namespace _2DSurviveGameServer._02Sys.Room.Actors
{
    using _2DSurviveGameServer._01Common;
    using _2DSurviveGameServer._02Sys.Room.FSM;
    using System;
    using System.Diagnostics;

    public class MonsterActor : Actor
    {
        public MonsterState monsterState { get; set; } = new MonsterState();
        public bool isStateChanged { get; set; } = false;
        private Vector2 targetPosition;
        private Random random = new Random();
        private float moveSpeed = 0.1f; // 移动速度
        private float moveDelay = 5f; // 移动延迟时间，单位为秒
        private Stopwatch stopwatch = new Stopwatch();
        private float detectRadius = 5.0f; // 检测玩家的半径
        private float attackRange = 1.0f; // 攻击范围
        private float fleeHealthThreshold = 5.0f; // 逃跑生命值阈值
        private float fleeDistance = 5.0f; // 逃跑距离
        private bool isFleeing = false; // 是否正在逃跑中
        private float fleeCooldownDuration = 5.0f; // 逃跑冷却时间，单位秒
        private Stopwatch fleeCooldownTimer = new Stopwatch();
        public bool hpChanged = false;

        public override void OnDestory()
        {
            // 停止计时器
            stopwatch.Stop();
            fleeCooldownTimer.Stop();
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
            fleeCooldownTimer.Start();
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

            // 检测周围的玩家并追逐或逃跑
            DetectAndReactToPlayers();

            MoveToTargetPosition();
        }

        private void SetRandomTargetPosition()
        {
            float x = (float)(random.NextDouble() * 20 - 10);
            float y = (float)(random.NextDouble() * 20 - 10);
            targetPosition = new Vector2(x, y);
            isStateChanged = true;
        }

        private void MoveToTargetPosition()
        {
            Vector2 direction = Vector2.Normalize(targetPosition - Body.Position);
            Vector2 newPosition = Body.Position + direction * moveSpeed;

            // 边界检查
            newPosition.X = MathHelper.Clamp(newPosition.X, -10f, 10f);
            newPosition.Y = MathHelper.Clamp(newPosition.Y, -10f, 10f);

            // 设置位置
            Body.Position = newPosition;

            // 更新怪物状态
            monsterState.pos = Body.Position.ToNetVector2();
            monsterState.dir = direction.ToNetVector2();
            isStateChanged = true;
        }

        private void DetectAndReactToPlayers()
        {
            var players = RoomStateFight.Instance.GetRoleActors();
            RoleActor nearestPlayer = null;
            float nearestDistance = float.MaxValue;

            // 找到距离最近的玩家
            foreach (var player in players)
            {
                float distance = Vector2.Distance(Body.Position, player.Body.Position);

                if (distance < nearestDistance)
                {
                    nearestPlayer = player;
                    nearestDistance = distance;
                }
            }

            if (nearestPlayer != null)
            {
                if (nearestDistance <= detectRadius)
                {
                    // 如果玩家在检测范围内，追逐玩家
                    targetPosition = nearestPlayer.Body.Position;

                    // 如果玩家在攻击范围内，执行攻击逻辑
                    if (nearestDistance <= attackRange)
                    {
                        AttackPlayer(nearestPlayer);
                    }
                }
                else if (monsterState.hp <= fleeHealthThreshold && !isFleeing && fleeCooldownTimer.Elapsed.TotalSeconds >= fleeCooldownDuration)
                {
                    // 生命值低于阈值时且不在逃跑状态且逃跑冷却时间到达时逃跑
                    Vector2 fleeDirection = Vector2.Normalize(Body.Position - nearestPlayer.Body.Position);
                    targetPosition = Body.Position + fleeDirection * fleeDistance;
                    isFleeing = true; // 设置逃跑状态
                    fleeCooldownTimer.Restart(); // 重置逃跑冷却计时器
                }
            }
            else
            {
                // 如果没有检测到玩家，则随机移动
                SetRandomTargetPosition();
            }
            isStateChanged = true;
        }

        private void AttackPlayer(RoleActor player)
        {
            // 这里可以实现攻击玩家的逻辑，比如减少玩家的HP
            //player.TakeDamage(5); // 假设每次攻击造成5点伤害
        }

        public void TakeDamage(int damage, long uid)
        {
            if (monsterState.hp <= 0)
            {
                return;
            }

            monsterState.hp -= damage;
            hpChanged = true;

            if (monsterState.hp <= 0)
            {
                foreach (var role in RoomStateFight.Instance.GetRoleActors())
                {
                    if (role.RoleState.uid == uid)
                    {
                        role.getIntegral();
                    }
                }
                monsterState.hp = 0;
            }
        }

        public bool isDead()
        {
            return monsterState.hp == 0;
        }
    }


}
