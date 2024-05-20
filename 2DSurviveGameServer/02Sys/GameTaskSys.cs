using _2DSurviveGameServer._01Common;

namespace _2DSurviveGameServer._02Sys
{
    public class GameTask
    {
        public string Name { get; set; }
        public string Task { get; set; }
        public bool Reward { get; set; }

        public GameTask(string name, string task, bool reward)
        {
            Name = name;
            Task = task;
            Reward = reward;
        }
    }
    public class GameTaskSys: SysRoot<GameTaskSys>
    {
        private List<GameTask> tasks;
        public override void Init()
        {
            tasks = new List<GameTask>();
            tasks.Add(new GameTask("任务1", "每日登录游戏", true));
        }
        public List<GameTask> GetTasks()
        {
            return tasks;
        }
        public void AddTask(GameTask task)
        {
            tasks.Add(task);
        }

    }
}
