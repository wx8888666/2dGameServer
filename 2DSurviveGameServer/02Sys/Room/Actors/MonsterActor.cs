using _2DSurviveGameServer.Helpers;
using FarseerPhysics.Dynamics;
using GameEngine;
using Microsoft.Xna.Framework;
using Protocol.Body;

namespace _2DSurviveGameServer._02Sys.Room.Actors
{
    using _2DSurviveGameServer._01Common;
    using _2DSurviveGameServer._01Common.Config;
    using _2DSurviveGameServer._02Sys.Room.FSM;
    using Protocol;
    using System;
    using System.Diagnostics;

    public class MonsterActor : Actor
    {
        public MonsterState monsterState { get; set; } = new MonsterState();
        public bool isStateChanged { get; set; } = false;

        private AStarMap map;
        private NetVector2 bornPos;
        private NetVector2 moveTarget;
        private List<AStarNode> path;
        private int pathIndex = 0;
        private float findPathRange = 500f;
        private int findPathTimeCount = 1000;
        private int findPathTimeCounter = 0;
        private Task<FindPathResult> findPathTask;
        private NetVector2 targetPos;
        private Stopwatch stopwatch = new Stopwatch();
        private NetVector2 LastTargetpos=null;
        public bool hpChanged = false; // 血量变化标志
        /// <summary>
        /// 攻击范围
        /// </summary>
        private float attackRange = 1.5f;
        /// <summary>
        /// 攻击间隔计时器
        /// </summary>
        private int attackTimeCounter = 0;
        /// <summary>
        /// 攻击间隔
        /// </summary>
        private int attackTimeCount = 1000;
        /// <summary>
        /// 攻击伤害
        /// </summary>
        private int attackDamage = 100;


        public MonsterActor()
        {

        }
        public void Init(AStarMap map)
        {
            this.map = map;
        }
        public override void OnDestory()
        {
            stopwatch.Stop();
        }

        public override void Start()
        {
            monsterState.id = this.Id;
            monsterState.Maxhp = 20;
            monsterState.hp = 20;
            monsterState.state = StateEnum.Alive;
            bornPos = monsterState.pos;
            monsterState.startPos = monsterState.endPos= bornPos;

            stopwatch.Start();
        }

        public override void Update()
        {
            int delta = 33;
            if (findPathTimeCounter < findPathTimeCount)
            {
                findPathTimeCounter += delta;
            }

            if (attackTimeCounter < attackTimeCount)
            {
                attackTimeCounter += delta;

            }
            if (FindRangeMinDistanceAliveRole(attackRange, out RoleActor role))
            {
                if (attackTimeCounter >= attackTimeCount)
                {
                    int baoji = new Random().Next(0, 500);

                    attackTimeCounter = 0;
                }
                if (path != null)
                {
                    path.Clear();
                    path = null;
                }

                findPathTask = null;

                monsterState.dir = NetVector2.Zero;
            }
            else
            {
                if (findPathTimeCounter >= findPathTimeCount)
                {
                    //判定当前寻路任务是否已经完成
                    if (findPathTask == null)
                    {
                        //如果找到了最小范围内最近的单位则进行寻路
                        if (FindRangeMinDistanceAliveRolePos(findPathRange, out targetPos))
                        {   if (LastTargetpos != null&&LastTargetpos == targetPos) return;
                            LastTargetpos = targetPos;
                            findPathTask = FindPath(targetPos);
                            monsterState.startPos = monsterState.pos;
                            monsterState.endPos = targetPos;
                            isStateChanged = true;
                        }
                        else {
                            this.Log("【Enemy】没有找到目标");
                        }
                        findPathTimeCounter = 0;
                    }

                    if (findPathTask != null && findPathTask.IsCompleted)
                    {
                        pathIndex = 0;
                        if (path != null)
                        {
                            path.Clear();
                        }
                        //ai卡在在死角了
                        if (findPathTask.Result.res == -1)
                        {
                            //重置ai位置
                            monsterState.pos = bornPos;
                        }
                        //正常寻路成功
                        if (findPathTask.Result.res == 0)
                        {
                            path = findPathTask.Result.path;
                        }

                        findPathTask = null;
                    }
                    //如果有路线，则进行移动
                    if (path != null && path.Count > 0 && pathIndex < path.Count)
                    {
                        AStarNode node = path[pathIndex];

                        if (MoveTo(node.worldPos.ToNetVector2(), delta))
                        {
                            pathIndex++;
                            //判断是否已经走到目的地，到达目的地则同步一次位置
                            if (pathIndex >= path.Count)
                            {
                                monsterState.startPos = NetVector2.Zero;
                                monsterState.endPos = NetVector2.Zero;
                                isStateChanged = true;
                            }
                        }

                        //this.Log($"【Enemy】Id:{enemyState.id} 当前位置：{enemyState.pos} 目标点：{moveTarget}");
                    }
                }
            }
           
        }
        private bool FindRangeMinDistanceAliveRolePos(float range, out NetVector2 minPos)
        {
            minPos = NetVector2.Zero;
            bool res = FindRangeMinDistanceAliveRole(range, out RoleActor role);
            if (res)
            {
                minPos = role.RoleState.pos;
            }
            return res;
        }
        //ref 与out 与c++中的&和*类似
        private bool FindRangeMinDistanceAliveRole(float range, out RoleActor role)
        {
            float minDistance = 99999999;
            role = null;
            bool isFind = false;
            for (int i = RoomStateFight.Instance.GetRoleActors().Count - 1; i >= 0; i--)
            {
                var v = RoomStateFight.Instance.GetRoleActors()[i];

                float distance = NetVector2.Distance(monsterState.pos, v.RoleState.pos);
                if (distance < range)
                {
                    if (isFind)
                    {
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            role = v;
                        }
                    }
                    else
                    {
                        minDistance = distance;
                        role = v;
                        isFind = true;
                    }
                }
            }

            return isFind;
        }

        private class FindPathResult
        {
            public int res;
            public List<AStarNode> path;
            public FindPathResult(int res, List<AStarNode> path)
            {
                this.res = res;
                this.path = path;
            }
        }

        private Task<FindPathResult> FindPath(NetVector2 endPos)
        {
            return Task.Run(() =>
            {
                List<AStarNode> path;
                int res = map.FindPath(monsterState.pos.ToVector2(), endPos.ToVector2(), out path);
                //this.Log(res);
                FindPathResult result = new FindPathResult(res, path);
                return result;
            });
        }

        private bool MoveTo(NetVector2 target, int delta)
        {
            moveTarget = target;
            Body.Position = NetVector2.MoveTowards(monsterState.pos, moveTarget, (delta / 1000f) * 1.0f).ToVector2();
            monsterState.dir = (target.ToVector2() - monsterState.pos.ToVector2()).ToNetVector2();
            monsterState.pos=Body.Position.ToNetVector2();
           
            return NetVector2.Distance(monsterState.pos, target) <= 0.01f;
           
        }
         void UpdateState()
        {
            
        }
        public bool isDead()
        {
            return monsterState.hp <= 0;
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

    }
}

    //public enum State 
    //{
    //    Idle,
    //    Patrol,
    //    Chase,
    //    Attack,
    //    Flee
    //}
    //public class MonsterActor : Actor
    //{
    //    private readonly object _lock = new object();
    //    public MonsterState monsterState { get; set; } = new MonsterState();
    //    public bool isStateChanged { get; set; } = false;
    //    private Vector2 targetPosition;
    //    private Random random = new Random();
    //    private float moveSpeed = 0.1f; // 移动速度
    //    private float moveDelay = 5f; // 移动延迟时间，单位为秒
    //    private Stopwatch stopwatch = new Stopwatch();
    //    private float detectRadius = 5.0f; // 检测玩家的半径
    //    private float attackRange = 1.0f; // 攻击范围
    //    private float fleeHealthThreshold = 5.0f; // 逃跑生命值阈值
    //    private float fleeDistance = 5.0f; // 逃跑距离
    //    private bool isFleeing = false; // 是否正在逃跑中
    //    private float fleeCooldownDuration = 5.0f; // 逃跑冷却时间，单位秒
    //    //逃跑计数器
    //    private Stopwatch fleeCooldownTimer = new Stopwatch();
    //    public bool hpChanged = false;
    //    private State currentState=State.Idle;
    //    public override void OnDestory()
    //    {
    //        // 停止计时器
    //        stopwatch.Stop();
    //        fleeCooldownTimer.Stop();
    //    }

    //    public override void Start()
    //    {
    //        monsterState.id = this.Id;
    //        monsterState.Maxhp = 20;
    //        monsterState.hp = 20;
    //        monsterState.state = StateEnum.Alive;
    //        SetRandomTargetPosition();

    //        // 启动计时器
    //        stopwatch.Start();
    //        fleeCooldownTimer.Start();
    //    }

    //    public override void Update()
    //    {

    //             switch (currentState)
    //            {
    //                case State.Idle:
    //                    Idle();
    //                    break;
    //                case State.Patrol:
    //                    Patrol();
    //                    break;
    //                case State.Chase:
    //                    Chase();
    //                    break;
    //                case State.Attack:
    //                    Attack();
    //                    break;
    //                case State.Flee:
    //                    Flee();
    //                    break;
    //            }

    //          MoveToTargetPosition();

    //    }
    //    private void Idle()
    //    {
    //        //空闲状态
    //        if (stopwatch.Elapsed.TotalSeconds >= moveDelay)
    //        {
    //            currentState = State.Patrol;
    //        }
    //    }
    //    private void Patrol()
    //    {
    //        //巡逻状态
    //        SetRandomTargetPosition();
    //        if (DetectPlayersInRadius(detectRadius))
    //        {
    //            currentState = State.Chase;
    //        }

    //        stopwatch.Restart();
    //    }

    //    private void Chase()
    //    {
    //        // 追逐状态逻辑
    //        RoleActor nearestPlayer = GetNearestPlayer();
    //        if (nearestPlayer != null)
    //        {
    //            targetPosition = nearestPlayer.Body.Position;
    //            if (Vector2.Distance(Body.Position, nearestPlayer.Body.Position) <= attackRange)
    //            {
    //                currentState = State.Attack;
    //            }
    //            else if (monsterState.hp <= fleeHealthThreshold && fleeCooldownTimer.Elapsed.TotalSeconds >= fleeCooldownDuration)
    //            {
    //                currentState = State.Flee;
    //                fleeCooldownTimer.Restart();
    //            }
    //        }
    //        else
    //        {
    //            currentState = State.Patrol;
    //        }
    //    }
    //    private void Attack()
    //    {
    //        RoleActor nearestPlayer = GetNearestPlayer();
    //        if (nearestPlayer != null && Vector2.Distance(Body.Position, nearestPlayer.Body.Position) <= attackRange)
    //        {
    //            // 执行攻击逻辑
    //            // nearestPlayer.TakeDamage(5);
    //        }
    //        else
    //        {
    //            currentState = State.Chase;
    //        }
    //    }
    //    private void Flee()
    //    {
    //        // 逃跑状态逻辑
    //        RoleActor nearestPlayer = GetNearestPlayer();
    //        if (nearestPlayer != null)
    //        {
    //            Vector2 fleeDirection = Vector2.Normalize(Body.Position - nearestPlayer.Body.Position);
    //            targetPosition = Body.Position + fleeDirection * fleeDistance;
    //        }

    //        isFleeing = true;
    //        if (Vector2.Distance(Body.Position, targetPosition) <= 0.1f)
    //        {
    //            isFleeing = false;
    //            currentState = State.Patrol;
    //        }

    //    }
    //    private RoleActor GetNearestPlayer()
    //    {
    //        var players = RoomStateFight.Instance.GetRoleActors();
    //        RoleActor nearestPlayer = null;
    //        float nearestDistance = float.MaxValue;

    //        foreach (var player in players)
    //        {
    //            float distance = Vector2.Distance(Body.Position, player.Body.Position);
    //            if (distance < nearestDistance)
    //            {
    //                nearestPlayer = player;
    //                nearestDistance = distance;
    //            }
    //        }
    //        return nearestPlayer;
    //    }
    //    private void SetRandomTargetPosition()
    //    {
    //        float x = (float)(random.NextDouble() * 20 - 10);
    //        float y = (float)(random.NextDouble() * 20 - 10);
    //        targetPosition = new Vector2(x, y);
    //        isStateChanged = true;
    //    }

    //    private void MoveToTargetPosition()
    //    {
    //        lock (_lock)
    //        {
    //            Vector2 direction = Vector2.Normalize(targetPosition - Body.Position);
    //            Vector2 newPosition = Body.Position + direction * moveSpeed;

    //            // 边界检查
    //            newPosition.X = MathHelper.Clamp(newPosition.X, -10f, 10f);
    //            newPosition.Y = MathHelper.Clamp(newPosition.Y, -10f, 10f);

    //            // 设置位置
    //            Body.Position = newPosition;

    //            // 更新怪物状态
    //            monsterState.pos = Body.Position.ToNetVector2();
    //            monsterState.dir = direction.ToNetVector2();
    //            isStateChanged = true;
    //        }
    //    }
    //    private bool DetectPlayersInRadius(float radius)
    //    {
    //        var players = RoomStateFight.Instance.GetRoleActors();
    //        foreach (var player in players)
    //        {
    //            if (Vector2.Distance(Body.Position, player.Body.Position) <= radius)
    //            {
    //                return true;
    //            }
    //        }
    //        return false;
    //    }

    //    private void AttackPlayer(RoleActor player)
    //    {
    //        // 这里可以实现攻击玩家的逻辑，比如减少玩家的HP
    //        //player.TakeDamage(5); // 假设每次攻击造成5点伤害
    //    }

    //    public void TakeDamage(int damage, long uid)
    //    {
    //        lock (_lock)
    //        {
    //            if (monsterState.hp <= 0)
    //            {
    //                return;
    //            }

    //            monsterState.hp -= damage;
    //            hpChanged = true;

    //            if (monsterState.hp <= 0)
    //            {
    //                foreach (var role in RoomStateFight.Instance.GetRoleActors())
    //                {
    //                    if (role.RoleState.uid == uid)
    //                    {
    //                        role.getIntegral();
    //                    }
    //                }
    //                monsterState.hp = 0;
    //            }
    //        }
    //    }

    //    public bool isDead()
    //    {
    //        lock (_lock)
    //        {
    //            return monsterState.hp == 0;
    //        }
    //    }
    //}


//}
