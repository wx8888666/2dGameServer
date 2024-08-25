using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer._01Common.BTNode;
using _2DSurviveGameServer._01Common.Config;
using _2DSurviveGameServer._02Sys.Room.Actors;
using _2DSurviveGameServer._02Sys.Room.FSM;
using _2DSurviveGameServer.Helpers;
using FarseerPhysics.Dynamics;
using GameEngine;
using Protocol.Body;
using System.Diagnostics;

namespace _2DSurviveGameServer._02Sys.Room.Actors
{
    public class MonsterActor : Actor
    {
        // 定义委托
        public delegate bool CheckConditionDelegate();
        public delegate void ActionDelegate();


        // 公开属性
        public MonsterState monsterState { get; set; } = new MonsterState();
        public bool isStateChanged { get; set; } = false;

        private BTNode _behaviorTree;

        // 公共字段（供行为树节点访问）
        private AStarMap map;
        public NetVector2 bornPos;
        public NetVector2 moveTarget;
        public List<AStarNode> path;
        public int pathIndex = 0;
        public float findPathRange = 500f;
        public int findPathTimeCount = 1000;
        public int findPathTimeCounter = 0;
        public Task<FindPathResult> findPathTask;
        public NetVector2 targetPos;
        public Stopwatch stopwatch = new Stopwatch();
        public NetVector2 LastTargetpos = null;
        public bool hpChanged = false; // 血量变化标志
        public float attackRange = 1.5f;
        public int attackTimeCounter = 0;
        public int attackTimeCount = 1000;
        private int attackDamage = 100;

        // 行为树根节点
        private BTNode behaviorTree;

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
            monsterState.startPos = monsterState.endPos = bornPos;

            stopwatch.Start();

            // 初始化行为树
            var findPathNode = new FindPathNode(this);
            var moveNode = new MoveNode(this);
            var attackNode = new AttackNode(this);

            _behaviorTree = new SequenceNode(findPathNode, moveNode, attackNode);
        }

        public override void Update()
        {

            // 更新计时器
            int delta = 33;
            if (findPathTimeCounter < findPathTimeCount)
            {
                findPathTimeCounter += delta;
            }

            if (attackTimeCounter < attackTimeCount)
            {
                attackTimeCounter += delta;
            }

            // 执行行为树
            _behaviorTree?.Execute();
        }


        public bool FindRangeMinDistanceAliveRolePos(float range, out NetVector2 minPos)
        {
            minPos = NetVector2.Zero;
            bool res = FindRangeMinDistanceAliveRole(range, out RoleActor role);
            if (res)
            {
                minPos = role.RoleState.pos;
            }
            return res;
        }

        public bool FindRangeMinDistanceAliveRole(float range, out RoleActor role)
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

        public class FindPathResult
        {
            public int res;
            public List<AStarNode> path;
            public FindPathResult(int res, List<AStarNode> path)
            {
                this.res = res;
                this.path = path;
            }
        }

        public Task<FindPathResult> FindPath(NetVector2 endPos)
        {
            return Task.Run(() =>
            {
                //this.Log("寻路开始");
                List<AStarNode> path;
                int res = map.FindPath(monsterState.pos.ToVector2(), endPos.ToVector2(), out path);
                FindPathResult result = new FindPathResult(res, path);
                return result;
            });
        }

        public bool MoveTo(NetVector2 target, int delta)
        {
            moveTarget = target;
            Body.Position = NetVector2.MoveTowards(monsterState.pos, moveTarget, (delta / 1000f) * 1.0f).ToVector2();
            monsterState.dir = (target.ToVector2() - monsterState.pos.ToVector2()).ToNetVector2();
            monsterState.pos = Body.Position.ToNetVector2();
            //this.Log("id:" + monsterState.id + "pos:" + monsterState.pos);
            return NetVector2.Distance(monsterState.pos, target) <= 0.01f;
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

    //原来的怪物逻辑
    //public class MonsterActor : Actor
    //{
    //    public MonsterState monsterState { get; set; } = new MonsterState();
    //    public bool isStateChanged { get; set; } = false;

    //    private AStarMap map;
    //    private NetVector2 bornPos;
    //    private NetVector2 moveTarget;
    //    private List<AStarNode> path;
    //    private int pathIndex = 0;
    //    public float findPathRange = 500f;
    //    private int findPathTimeCount = 1000;
    //    private int findPathTimeCounter = 0;
    //    private Task<FindPathResult> findPathTask;
    //    private NetVector2 targetPos;
    //    private Stopwatch stopwatch = new Stopwatch();
    //    private NetVector2 LastTargetpos = null;
    //    public bool hpChanged = false; // 血量变化标志
    //                                   //攻击范围
    //    private float attackRange = 1.5f;
    //    // 攻击间隔计时器
    //    private int attackTimeCounter = 0;
    //    // 攻击间隔
    //    private int attackTimeCount = 1000;
    //    //攻击伤害
    //    private int attackDamage = 100;

    //    public void Init(AStarMap map)
    //    {
    //        this.map = map;
    //    }
    //    public override void OnDestory()
    //    {
    //        stopwatch.Stop();
    //    }

    //    public override void Start()
    //    {
    //        monsterState.id = this.Id;
    //        monsterState.Maxhp = 20;
    //        monsterState.hp = 20;
    //        monsterState.state = StateEnum.Alive;
    //        bornPos = monsterState.pos;
    //        monsterState.startPos = monsterState.endPos = bornPos;

    //        stopwatch.Start();
    //    }

    //    public override void Update()
    //    {
    //        int delta = 33;
    //        if (findPathTimeCounter < findPathTimeCount)
    //        {
    //            findPathTimeCounter += delta;
    //        }

    //        if (attackTimeCounter < attackTimeCount)
    //        {
    //            attackTimeCounter += delta;

    //        }
    //        if (FindRangeMinDistanceAliveRole(attackRange, out RoleActor role))
    //        {
    //            if (attackTimeCounter >= attackTimeCount)
    //            {
    //                int baoji = new Random().Next(0, 500);

    //                attackTimeCounter = 0;
    //            }
    //            if (path != null)
    //            {
    //                path.Clear();
    //                path = null;
    //            }

    //            findPathTask = null;

    //            monsterState.dir = NetVector2.Zero;
    //        }
    //        else
    //        {
    //            if (findPathTimeCounter >= findPathTimeCount)
    //            {
    //                //判定当前寻路任务是否已经完成
    //                if (findPathTask == null)
    //                {
    //                    //如果找到了最小范围内最近的单位则进行寻路
    //                    if (FindRangeMinDistanceAliveRolePos(findPathRange, out targetPos))
    //                    {
    //                        if (LastTargetpos != null && LastTargetpos == targetPos) return;
    //                        LastTargetpos = targetPos;
    //                        findPathTask = FindPath(targetPos);
    //                        monsterState.startPos = monsterState.pos;
    //                        monsterState.endPos = targetPos;
    //                        isStateChanged = true;
    //                    }
    //                    else
    //                    {
    //                        this.Log("【Enemy】没有找到目标");
    //                    }
    //                    findPathTimeCounter = 0;
    //                }

    //                if (findPathTask != null && findPathTask.IsCompleted)
    //                {
    //                    pathIndex = 0;
    //                    if (path != null)
    //                    {
    //                        path.Clear();
    //                    }
    //                    //ai卡在在死角了
    //                    if (findPathTask.Result.res == -1)
    //                    {
    //                        //重置ai位置
    //                        monsterState.pos = bornPos;
    //                    }
    //                    //正常寻路成功
    //                    if (findPathTask.Result.res == 0)
    //                    {
    //                        path = findPathTask.Result.path;
    //                    }

    //                    findPathTask = null;
    //                }
    //                //如果有路线，则进行移动
    //                if (path != null && path.Count > 0 && pathIndex < path.Count)
    //                {
    //                    AStarNode node = path[pathIndex];

    //                    if (MoveTo(node.worldPos.ToNetVector2(), delta))
    //                    {
    //                        pathIndex++;
    //                        //判断是否已经走到目的地，到达目的地则同步一次位置
    //                        if (pathIndex >= path.Count)
    //                        {
    //                            monsterState.startPos = NetVector2.Zero;
    //                            monsterState.endPos = NetVector2.Zero;
    //                            isStateChanged = true;
    //                        }
    //                    }

    //                    //this.Log($"【Enemy】Id:{enemyState.id} 当前位置：{enemyState.pos} 目标点：{moveTarget}");
    //                }
    //            }
    //        }

    //    }
    //    private bool FindRangeMinDistanceAliveRolePos(float range, out NetVector2 minPos)
    //    {
    //        minPos = NetVector2.Zero;
    //        bool res = FindRangeMinDistanceAliveRole(range, out RoleActor role);
    //        if (res)
    //        {
    //            minPos = role.RoleState.pos;
    //        }
    //        return res;
    //    }
    //    //ref 与out 与c++中的&和*类似
    //    public bool FindRangeMinDistanceAliveRole(float range, out RoleActor role)
    //    {
    //        float minDistance = 99999999;
    //        role = null;
    //        bool isFind = false;
    //        for (int i = RoomStateFight.Instance.GetRoleActors().Count - 1; i >= 0; i--)
    //        {
    //            var v = RoomStateFight.Instance.GetRoleActors()[i];

    //            float distance = NetVector2.Distance(monsterState.pos, v.RoleState.pos);
    //            if (distance < range)
    //            {
    //                if (isFind)
    //                {
    //                    if (distance < minDistance)
    //                    {
    //                        minDistance = distance;
    //                        role = v;
    //                    }
    //                }
    //                else
    //                {
    //                    minDistance = distance;
    //                    role = v;
    //                    isFind = true;
    //                }
    //            }
    //        }

    //        return isFind;
    //    }

    //    private class FindPathResult
    //    {
    //        public int res;
    //        public List<AStarNode> path;
    //        public FindPathResult(int res, List<AStarNode> path)
    //        {
    //            this.res = res;
    //            this.path = path;
    //        }
    //    }

    //    private Task<FindPathResult> FindPath(NetVector2 endPos)
    //    {
    //        return Task.Run(() =>
    //        {
    //            List<AStarNode> path;
    //            int res = map.FindPath(monsterState.pos.ToVector2(), endPos.ToVector2(), out path);
    //            //this.Log(res);
    //            FindPathResult result = new FindPathResult(res, path);
    //            return result;
    //        });
    //    }

    //    private bool MoveTo(NetVector2 target, int delta)
    //    {
    //        moveTarget = target;
    //        Body.Position = NetVector2.MoveTowards(monsterState.pos, moveTarget, (delta / 1000f) * 1.0f).ToVector2();
    //        monsterState.dir = (target.ToVector2() - monsterState.pos.ToVector2()).ToNetVector2();
    //        monsterState.pos = Body.Position.ToNetVector2();
    //        //this.Log(monsterState.pos.ToVector2().X);
    //        return NetVector2.Distance(monsterState.pos, target) <= 0.01f;

    //    }
    //    void UpdateState()
    //    {

    //    }
    //    public bool isDead()
    //    {
    //        return monsterState.hp <= 0;
    //    }
    //    public void TakeDamage(int damage, long uid)
    //    {

    //        if (monsterState.hp <= 0)
    //        {
    //            return;
    //        }

    //        monsterState.hp -= damage;
    //        hpChanged = true;

    //        if (monsterState.hp <= 0)
    //        {
    //            foreach (var role in RoomStateFight.Instance.GetRoleActors())
    //            {
    //                if (role.RoleState.uid == uid)
    //                {
    //                    role.getIntegral();
    //                }
    //            }
    //            monsterState.hp = 0;
    //        }

    //    }


    //}


}
