using FarseerPhysics.Dynamics;
using GameEngine;
using Protocol.Body;

namespace _2DSurviveGameServer._01Common.Skill
{
    // 基础技能类
    public abstract class Skill
    {
        public string SkillName { get; set; }
        public float Cooldown { get; set; } // 冷却时间
        private float lastUsedTime; // 上次使用时间

        public Skill(string name, float cooldown)
        {
            SkillName = name;
            Cooldown = cooldown;
            lastUsedTime = -cooldown; // 初始化为负值，确保第一次可以立刻使用
        }

        public bool CanUse(float currentTime)
        {
            return currentTime - lastUsedTime >= Cooldown;
        }

        public void Use(float currentTime)
        {
            if (CanUse(currentTime))
            {
                ExecuteSkill();
                lastUsedTime = currentTime;
            }
        }

        protected abstract void ExecuteSkill(); // 执行技能的具体逻辑
    }

    // 火球技能示例
    public class Fireball : Skill
    {
        public Fireball() : base("Fireball", 5.0f) { } // 5秒冷却时间

        protected override void ExecuteSkill()
        {
            // 执行火球技能的具体逻辑
            Console.WriteLine("Fireball skill executed!");
        }
    }

    // 冰冻技能示例
    public class IceBlast : Skill
    {
        public IceBlast() : base("IceBlast", 8.0f) { } // 8秒冷却时间

        protected override void ExecuteSkill()
        {
            // 执行冰冻技能的具体逻辑
            Console.WriteLine("IceBlast skill executed!");
        }
    }


}
