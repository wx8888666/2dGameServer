using _2DSurviveGameServer._02Sys.Room.Actors;

namespace _2DSurviveGameServer._01Common
{
    //public interface IBehavior
    //{
    //    void Update(MonsterActor monster);
    //}
    //public class BehaviorTree
    //{
    //    private IBehavior rootBehavior;
    //    public BehaviorTree(IBehavior initialBehavior) 
    //    {
    //        this.rootBehavior = initialBehavior;
    //    }
    //    public void Update(MonsterActor monster) 
    //    {
    //        rootBehavior.Update(monster);
    //    }
    //    public void ChangeBehavior(IBehavior newBehavior)
    //    {
    //        rootBehavior = newBehavior;
    //    }
    //}
    //public class PatrolBehavior : IBehavior
    //{
    //    public void Update(MonsterActor monster)
    //    {
    //        // 巡逻逻辑
    //        if (monster.FindRangeMinDistanceAliveRole(monster.findPathRange, out RoleActor target))
    //        {
    //            monster.behaviorTree.ChangeBehavior(new ChaseBehavior(target));
    //        }
    //    }
    //}

    //public class ChaseBehavior : IBehavior
    //{
    //    private RoleActor target;

    //    public ChaseBehavior(RoleActor target)
    //    {
    //        this.target = target;
    //    }

    //    public void Update(MonsterActor monster)
    //    {
    //        if (monster.TryAttack(target))
    //        {
    //            monster.behaviorTree.ChangeBehavior(new PatrolBehavior());
    //        }
    //        else
    //        {
    //            // 移动到目标的逻辑
    //            monster.MoveTo(target.RoleState.pos, 33);
    //        }
    //    }
    //}

    //public class AttackBehavior : IBehavior
    //{
    //    private RoleActor target;

    //    public AttackBehavior(RoleActor target)
    //    {
    //        this.target = target;
    //    }

    //    public void Update(MonsterActor monster)
    //    {
    //        if (monster.TryAttack(target))
    //        {
    //            // 如果攻击成功，返回到巡逻状态
    //            monster.behaviorTree.ChangeBehavior(new PatrolBehavior());
    //        }
    //    }
    //}

}
