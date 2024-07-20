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

    public enum State 
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Flee
    }
    public class MonsterActor : Actor
    {
        private readonly object _lock = new object();
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
        //逃跑计数器
        private Stopwatch fleeCooldownTimer = new Stopwatch();
        public bool hpChanged = false;
        private State currentState=State.Idle;
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
           
                 switch (currentState)
                {
                    case State.Idle:
                        Idle();
                        break;
                    case State.Patrol:
                        Patrol();
                        break;
                    case State.Chase:
                        Chase();
                        break;
                    case State.Attack:
                        Attack();
                        break;
                    case State.Flee:
                        Flee();
                        break;
                }

              MoveToTargetPosition();
            
        }
        private void Idle()
        {
            //空闲状态
            if (stopwatch.Elapsed.TotalSeconds >= moveDelay)
            {
                currentState = State.Patrol;
            }
        }
        private void Patrol()
        {
            //巡逻状态
            SetRandomTargetPosition();
            if (DetectPlayersInRadius(detectRadius))
            {
                currentState = State.Chase;
            }

            stopwatch.Restart();
        }

        private void Chase()
        {
            // 追逐状态逻辑
            RoleActor nearestPlayer = GetNearestPlayer();
            if (nearestPlayer != null)
            {
                targetPosition = nearestPlayer.Body.Position;
                if (Vector2.Distance(Body.Position, nearestPlayer.Body.Position) <= attackRange)
                {
                    currentState = State.Attack;
                }
                else if (monsterState.hp <= fleeHealthThreshold && fleeCooldownTimer.Elapsed.TotalSeconds >= fleeCooldownDuration)
                {
                    currentState = State.Flee;
                    fleeCooldownTimer.Restart();
                }
            }
            else
            {
                currentState = State.Patrol;
            }
        }
        private void Attack()
        {
            RoleActor nearestPlayer = GetNearestPlayer();
            if (nearestPlayer != null && Vector2.Distance(Body.Position, nearestPlayer.Body.Position) <= attackRange)
            {
                // 执行攻击逻辑
                // nearestPlayer.TakeDamage(5);
            }
            else
            {
                currentState = State.Chase;
            }
        }
        private void Flee()
        {
            // 逃跑状态逻辑
            RoleActor nearestPlayer = GetNearestPlayer();
            if (nearestPlayer != null)
            {
                Vector2 fleeDirection = Vector2.Normalize(Body.Position - nearestPlayer.Body.Position);
                targetPosition = Body.Position + fleeDirection * fleeDistance;
            }

            isFleeing = true;
            if (Vector2.Distance(Body.Position, targetPosition) <= 0.1f)
            {
                isFleeing = false;
                currentState = State.Patrol;
            }

        }
        private RoleActor GetNearestPlayer()
        {
            var players = RoomStateFight.Instance.GetRoleActors();
            RoleActor nearestPlayer = null;
            float nearestDistance = float.MaxValue;

            foreach (var player in players)
            {
                float distance = Vector2.Distance(Body.Position, player.Body.Position);
                if (distance < nearestDistance)
                {
                    nearestPlayer = player;
                    nearestDistance = distance;
                }
            }
            return nearestPlayer;
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
        private bool DetectPlayersInRadius(float radius)
        {
            var players = RoomStateFight.Instance.GetRoleActors();
            foreach (var player in players)
            {
                if (Vector2.Distance(Body.Position, player.Body.Position) <= radius)
                {
                    return true;
                }
            }
            return false;
        }

        private void AttackPlayer(RoleActor player)
        {
            // 这里可以实现攻击玩家的逻辑，比如减少玩家的HP
            //player.TakeDamage(5); // 假设每次攻击造成5点伤害
        }

        public void TakeDamage(int damage, long uid)
        {
            lock (_lock)
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
        }

        public bool isDead()
        {
            lock (_lock)
            {
                return monsterState.hp == 0;
            }
        }
    }


}
