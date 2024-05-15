using _2DSurviveGameServer._01Common;
using PETimer;

namespace _2DSurviveGameServer._03Svc
{
    public class TimerSvc:SvcRoot<TimerSvc>
    {
        TickTimer tickTimer;

        public override void Init()
        {
            base.Init();
            tickTimer = new TickTimer(0,false);
        }

        public override void Update()
        {
            base.Update();
            tickTimer.UpdateTask();
        }

        public int AddTask(int delay,Action callback,int count = 1)
        {
            return tickTimer.AddTask((uint)delay, (i) => { callback(); }, null,count);
        }

        public void DeleteTask(int id)
        {
            tickTimer.DeleteTask(id);
        }
    }
}
