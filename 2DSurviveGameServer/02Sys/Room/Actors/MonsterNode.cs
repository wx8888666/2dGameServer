using _2DSurviveGameServer._01Common.BTNode;
using _2DSurviveGameServer._01Common.Config;
using _2DSurviveGameServer.Helpers;
using Protocol.Body;

namespace _2DSurviveGameServer._02Sys.Room.Actors
{
    public class FindPathNode : BTNode
    {
        private MonsterActor _monster;

        public FindPathNode(MonsterActor monster)
        {
            _monster = monster;
        }

        public override NodeState Execute()
        {
            //this.Log("NOdeStae");
            if (_monster.findPathTask == null || _monster.findPathTask.IsCompleted)
            {
                if (_monster.FindRangeMinDistanceAliveRolePos(_monster.findPathRange, out _monster.targetPos))
                {
                    if (_monster.LastTargetpos != null && _monster.LastTargetpos == _monster.targetPos)
                        return NodeState.Success;

                    _monster.LastTargetpos = _monster.targetPos;
                    _monster.findPathTask = _monster.FindPath(_monster.targetPos);
                    _monster.monsterState.startPos = _monster.monsterState.pos;
                    _monster.monsterState.endPos = _monster.targetPos;
                    _monster.isStateChanged = true;
                }
                else
                {
                    return NodeState.Failure;
                }
            }

            if (_monster.findPathTask.IsCompleted)
            {
                if (_monster.findPathTask.Result.res == -1)
                {
                    _monster.monsterState.pos = _monster.bornPos;
                    _monster.path = null;
                }
                else if (_monster.findPathTask.Result.res == 0)
                {
                    _monster.path = _monster.findPathTask.Result.path;
                }
                _monster.findPathTask = null;
                return _monster.path != null && _monster.path.Count > 0 ? NodeState.Success : NodeState.Failure;
            }

            return NodeState.Running;
        }
    }

    public class MoveNode : BTNode
    {
        private MonsterActor _monster;

        public MoveNode(MonsterActor monster)
        {
            _monster = monster;
        }

        public override NodeState Execute()
        {
            //this.Log("MoveStae");
            if (_monster.path != null && _monster.path.Count > 0 && _monster.pathIndex < _monster.path.Count)
            {
                //this.Log("MoveStae");
                AStarNode node = _monster.path[_monster.pathIndex];
                if (_monster.MoveTo(node.worldPos.ToNetVector2(), 33))
                {
                    this.Log(_monster.monsterState.pos);
                    _monster.pathIndex++;
                    if (_monster.pathIndex >= _monster.path.Count)
                    {
                        _monster.monsterState.startPos = NetVector2.Zero;
                        _monster.monsterState.endPos = NetVector2.Zero;
                        _monster.isStateChanged = true;
                    }
                }
                return NodeState.Running;
            }
            return NodeState.Success;
        }
    }

    public class AttackNode : BTNode
    {
        private MonsterActor _monster;

        public AttackNode(MonsterActor monster)
        {
            _monster = monster;
        }

        public override NodeState Execute()
        {
            if (_monster.FindRangeMinDistanceAliveRole(_monster.attackRange, out RoleActor role))
            {
                if (_monster.attackTimeCounter >= _monster.attackTimeCount)
                {
                    int baoji = new Random().Next(0, 500);
                    _monster.attackTimeCounter = 0;
                    return NodeState.Success;
                }
                return NodeState.Running;
            }
            return NodeState.Failure;
        }
    }




}
