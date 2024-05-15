using _2DSurviveGameServer._03Svc;
using _2DSurviveGameServer._04Interface;

namespace _2DSurviveGameServer._01Common
{
    public class SysRoot<T> : Singleton<T>, IComponent where T : new()
    {
        protected NetSvc netSvc;
        protected CacheSvc cacheSvc;

        public virtual void Init()
        {
            netSvc = NetSvc.Instance;
            cacheSvc = CacheSvc.Instance;
        }

        public virtual void Update()
        {
        }
    }
}
