namespace _2DSurviveGameServer._01Common.BTNode
{
    public enum NodeState
    {
        Running,
        Success,
        Failure
    }
    public abstract class BTNode
    {
        public abstract NodeState Execute();
    }
    //Selector节点
    //作用：从左到右依次执行子节点，直到有一个子节点返回 Success，
    //Selector 返回 Success。如果所有子节点都返回 Failure，则 Selector 返回 Failure。
    public class SelectorNode : BTNode
    {
        private List<BTNode> _nodes = new List<BTNode>();

        public SelectorNode(params BTNode[] nodes)
        {
            _nodes.AddRange(nodes);
        }

        public override NodeState Execute()
        {
            foreach (var node in _nodes)
            {
                var result = node.Execute();
                if (result != NodeState.Failure)
                {
                    return result;
                }
            }
            return NodeState.Failure;
        }
    }
    //Sequence 节点
    //作用：从左到右依次执行子节点，直到有一个子节点返回 Failure，
    // Sequence 返回 Failure。如果所有子节点都返回 Success，则 Sequence 返回 Success。
    public class SequenceNode : BTNode
    {
        private List<BTNode> _nodes = new List<BTNode>();

        public SequenceNode(params BTNode[] nodes)
        {
            _nodes.AddRange(nodes);
        }

        public override NodeState Execute()
        {
            foreach (var node in _nodes)
            {
                var result = node.Execute();
                if (result != NodeState.Success)
                {
                    return result;
                }
            }
            return NodeState.Success;
        }
    }
    //并行节点（Parallel Node）
    //作用：并行执行所有子节点，只有当所有子节点都返回 Success 时，
    // Parallel 节点才返回 Success。如果有任何一个子节点返回 Failure，则立即返回 Failure。
    public class Parallel : BTNode
    {
        private List<BTNode> _children = new List<BTNode>();

        public void AddChild(BTNode node)
        {
            _children.Add(node);
        }

        public override NodeState Execute()
        {
            bool isAnyNodeRunning = false;

            foreach (var child in _children)
            {
                NodeState result = child.Execute();
                if (result == NodeState.Failure)
                {
                    return NodeState.Failure;
                }
                if (result == NodeState.Running)
                {
                    isAnyNodeRunning = true;
                }
            }

            return isAnyNodeRunning ? NodeState.Running : NodeState.Success;
        }
    }
       //条件节点（Condition Node）
       //作用：判断某个条件是否满足。
       // 通常返回 Success 或 Failure，不会返回 Running。比如，检查敌人是否在攻击范围内。
    public class Condition : BTNode
    {
        private Func<bool> _condition;

        public Condition(Func<bool> condition)
        {
            _condition = condition;
        }

        public override NodeState Execute()
        {
            return _condition() ? NodeState.Success : NodeState.Failure;
        }
    }
    //动作节点（Action Node）
    //作用：执行一个具体的动作，
    //比如移动到某个位置、攻击目标等。通常会返回 Running，直到动作完成后返回 Success 或 Failure。
    public class ActionNode : BTNode
    {
        private Func<NodeState> _action;

        public ActionNode(Func<NodeState> action)
        {
            _action = action;
        }

        public override NodeState Execute()
        {
            return _action();
        }
    }
   // 重复节点（Repeat Node）
   //作用：重复执行一个子节点，直到它返回 Success 或 Failure。
    public class Repeat : BTNode
    {
        private BTNode _child;

        public Repeat(BTNode child)
        {
            _child = child;
        }

        public override NodeState Execute()
        {
            while (true)
            {
                NodeState result = _child.Execute();
                if (result != NodeState.Running)
                {
                    return result;
                }
            }
        }
    }




}
