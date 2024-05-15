using _2DSurviveGameServer._04Interface;

namespace _2DSurviveGameServer._01Common
{
    public class SvcRoot<T> : Singleton<T>, IComponent where T : new()
    {
        public virtual void Init()
        {
        }

        public virtual void Update()
        {
        }
    }
}
