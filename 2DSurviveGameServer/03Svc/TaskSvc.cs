using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer.Helpers;
using Newtonsoft.Json;
using Protocol.Body;
using StackExchange.Redis;
using System.Text;
using static _2DSurviveGameServer._01Common.Friendrequest;

namespace _2DSurviveGameServer._03Svc
{
    public class TaskSvc : SvcRoot<TaskSvc>
    {
        private readonly IDatabase redisDb;
        public TaskSvc()
        {
            redisDb = RedisManager.GetDatabase();
        }
        public void InsertTasksFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found.");
                return;
            }

            string jsonContent = File.ReadAllText(filePath, Encoding.UTF8);
            List<TaskType> tasks = JsonConvert.DeserializeObject<List<TaskType>>(jsonContent);

            foreach (var task in tasks)
            {
                // 检查数据库中是否已存在相同描述的任务
                bool exists = SqlSugarHelper.Db.Queryable<TaskType>().Any(t => t.Description == task.Description);

                if (!exists)
                {
                    // 如果不存在，则执行插入操作
                    SqlSugarHelper.Db.Insertable(task).ExecuteCommand();

                }
                else
                {
                }
            }

        }

        public bool AreTasksAlreadyInserted()
        {
            return SqlSugarHelper.Db.Queryable<TaskType>().Any();
        }

        public override void Init()
        {
            base.Init();
            // Assuming "path/to/tasks.json" is the relative path to your tasks file.
            string filePath = "data/tasks.json";
            InsertTasksFromFile(filePath);
           
        }
        //随机在任务库中获取任务
        public List<TaskType> GetRandomTasks(int count)
        {
            var allTasks = SqlSugarHelper.Db.Queryable<TaskType>().ToList();
            Random random = new Random();
            var randomTasks = allTasks.OrderBy(x => random.Next()).Take(count).ToList();
            return randomTasks;
        }
        public void AssignDailyTasks(long userId)
        {
            string userTasksKey = $"user:{userId}:tasks";

            if (redisDb.KeyExists(userTasksKey))
            {
                Console.WriteLine("User already has tasks assigned.");
                return;
            }

            var randomTasks = GetRandomTasks(4);

            var userTasks = randomTasks.Select(task => new UserTask
            {
                Description = task.Description,
                Reward = task.Reward,
                RewardNumber = task.RewardNumber,
                Condition = task.Condition,
                IsCompleted = false
            }).ToArray();
            //通过redis缓存每日任务
            string serializedTasks = JsonConvert.SerializeObject(userTasks);
            redisDb.StringSet(userTasksKey, serializedTasks, TimeSpan.FromDays(1));

        }
        public List<UserTask> GetUserTasks(long userId)
        {
            string userTasksKey = $"user:{userId}:tasks";
            string serializedTasks = redisDb.StringGet(userTasksKey);

            if (string.IsNullOrEmpty(serializedTasks))
            {
                return new List<UserTask>();
            }

            return JsonConvert.DeserializeObject<List<UserTask>>(serializedTasks);
        }
        public void UpdateTaskStatus(long userId, string description, bool isCompleted)
        {
            string userTasksKey = $"user:{userId}:tasks";
            string serializedTasks = redisDb.StringGet(userTasksKey);

            if (!string.IsNullOrEmpty(serializedTasks))
            {
                var userTasks = JsonConvert.DeserializeObject<List<UserTask>>(serializedTasks);
                var task = userTasks.FirstOrDefault(t => t.Description == description);
                if (task != null)
                {
                    task.IsCompleted = isCompleted;
                    serializedTasks = JsonConvert.SerializeObject(userTasks);
                    redisDb.StringSet(userTasksKey, serializedTasks, TimeSpan.FromDays(1));
                    Console.WriteLine($"Task status updated: {task.Description} is now {(isCompleted ? "completed" : "not completed")}");
                }
            }
        }
    }
}
