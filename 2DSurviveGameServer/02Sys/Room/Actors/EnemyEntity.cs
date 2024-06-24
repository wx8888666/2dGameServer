using _2DSurviveGameServer._01Common.Astar;
using _2DSurviveGameServer._02Sys.Room.FSM;
using _2DSurviveGameServer.Helpers;
using FarseerPhysics.Dynamics;
using GameEngine;
using Microsoft.Xna.Framework;
using Protocol;
using Protocol.Body;

namespace _2DSurviveGameServer._02Sys.Room.Actors
{
    public class EnemyEntity : Actor
    {
        public EnemyState EnemyState { get; set; } = new EnemyState();
        public  bool isStateChanged = false;
        private AStarMap map;
        private float attackRange = 1.5f;
        private int attackTimeCounter = 0;
        private int attackTimeCount = 1000;
        private int attackDamage = 100;
        private NetVector2 targetPos;
        private List<AStarNode> path;
        private int pathIndex = 0;
        private Task<FindPathResult> findPathTask;
        private int findPathTimeCounter = 0;
        private int findPathTimeCount = 1000;
        private float findPathRange = 500f;
        private RoomStateFight room;


        public override void Start()
        {
            
            
        }
        public void Init(RoomStateFight room, AStarMap map, int id,EnemyState enemyState)
        {
            this.room = room;
            this.map = map;
            this.Id = id;
            this.EnemyState = enemyState;
        }
        public override void Update()
        {
            int delta = 33; // Assume a fixed delta time for simplicity
            if (IsAlive())
            {
                UpdateAI(delta);
            }
        }

        private void UpdateAI(int delta)
        {
            if (findPathTimeCounter < findPathTimeCount)
            {
                findPathTimeCounter += delta;
            }

            //if (attackTimeCounter < attackTimeCount)
            //{
            //    attackTimeCounter += delta;
            //}

            if (IsInRange(attackRange, out RoleActor role))
            {
                Attack(role);
            }
            else
            {
                if (findPathTimeCounter >= findPathTimeCount)
                {
                    if (findPathTask == null && FindRangeMinDistanceAliveRolePos(findPathRange, out targetPos))
                    {
                        findPathTask = FindPath(targetPos);
                    }

                    findPathTimeCounter = 0;
                }

                if (findPathTask != null && findPathTask.IsCompleted)
                {
                    pathIndex = 0;
                    path = findPathTask.Result.path;
                    findPathTask = null;
                    isStateChanged = true;
                }

                if (path != null && pathIndex < path.Count)
                {
                    MoveToTarget(delta);
                }
            }
        }

        private bool IsInRange(float range, out RoleActor role)
        {
            // Implement the logic to check if a role is within the specified range
            role = null;
            return false;
        }

        private void Attack(RoleActor role)
        {
            if (attackTimeCounter >= attackTimeCount)
            {
                int baoji = new Random().Next(0, 500);
                //角色受伤逻辑
                attackTimeCounter = 0;
                isStateChanged = true; // 攻击
            }
        }

        private bool FindRangeMinDistanceAliveRolePos(float range, out NetVector2 minPos)
        {
           
            minPos = NetVector2.Zero;
            bool res = FindRangeMinDistanceAliveRole(range, out RoleActor role);
            if (res)
            {
                minPos=role.RoleState.pos;
                isStateChanged = true; // 目标位置更新
            }
            return false;
        }
        private bool FindRangeMinDistanceAliveRole(float range, out RoleActor role)
        {
            float minDistance = 99999999;
            role = null;
            bool isFind = false;
            for (int i = room.roleActorList.Count - 1; i >= 0; i--)
            {
                var v = room.roleActorList[i];

                //如果已经死亡则跳过
                if (v.RoleState.hp<=0)
                {
                    continue;
                }
                float distance = NetVector2.Distance(EnemyState.pos, v.RoleState.pos);
                //是否在范围内
                if (distance < range)
                {
                    //判定是否是第一个找到的
                    if (isFind)
                    {
                        //是否是最小的
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
        private Task<FindPathResult> FindPath(NetVector2 endPos)
        {
            return Task.Run(() =>
            {
                List<AStarNode> path;
                int res = map.FindPath(EnemyState.pos.ToVector2(), endPos.ToVector2(), out path);
                return new FindPathResult(res, path);
            });
        }

        private void MoveToTarget(int delta)
        {
            if (path != null && path.Count > 0 && pathIndex < path.Count)
            {
                AStarNode node = path[pathIndex];
                if (MoveTo(node.worldPos.ToNetVector2(), delta))
                {
                    pathIndex++;
                    isStateChanged = true; // 移动
                }
            }
        }

        private bool MoveTo(NetVector2 target, int delta)
        {
            EnemyState.pos = NetVector2.MoveTowards(EnemyState.pos, target, (delta / 1000f) * EnemyState.speed);
            return NetVector2.Distance(EnemyState.pos, target) <= 0.01f;
        }

        public void Damage(long attackId, int damage, EntityTypeEnum entityType)
        {
            if (EnemyState.hp == 0)
            {
                return;
            }

            EnemyState.hp -= damage;
            isStateChanged = true; // 受伤
            if (EnemyState.hp <= 0)
            {
                EnemyState.hp = 0;
                EnemyState.entityState = EntityStateEnum.Dead;
                // 广播死亡逻辑
                OnDestory();
            }
            else
            {
                // 广播受伤逻辑
            }
        }

        public override void OnDestory()
        {
            //销毁敌人代码
        }

        private bool IsAlive()
        {
            return EnemyState.entityState != EntityStateEnum.Dead;
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
       
    }
}