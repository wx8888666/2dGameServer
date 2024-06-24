using _2DSurviveGameServer._01Common;
using _2DSurviveGameServer.Helpers;
using Newtonsoft.Json;
using Protocol.Body;
using Protocol.DBModel;
using StackExchange.Redis;
using System.Text;
using System.Threading.Tasks;
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
                //Console.WriteLine("File not found.");
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
            string filePath = "Config/tasks.json";
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
                //Console.WriteLine("User already has tasks assigned.");
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
                    //if (task.IsCompleted) return;
                    task.IsCompleted = isCompleted;
                    serializedTasks = JsonConvert.SerializeObject(userTasks);
                    redisDb.StringSet(userTasksKey, serializedTasks, TimeSpan.FromDays(1));
                    //Console.WriteLine($"Task status updated: {task.Description} is now {(isCompleted ? "completed" : "not completed")}");

                    // 查询该角色是否有背包数据
                    var existingBag = SqlSugarHelper.Db.Queryable<Bag>().First(p => p.UId == userId);

                    // 根据任务的奖励类型以及数量直接增加数据
                    if (isCompleted && existingBag != null)
                    {
                        HandleTaskReward(existingBag, task);
                    }
                    // 如果不存在对应 UId 的 Bag 数据，创建一个新的 Bag 对象并插入数据库
                    else if (isCompleted && existingBag == null)
                    {
                        CreateNewBagAndHandleReward(userId, task);
                    }
                }
            }
        }

        private void HandleTaskReward(Bag existingBag, UserTask task)
        {
            // 处理任务奖励，根据奖励类型和数量增加到背包中
            switch (task.Reward)
            {
                case "金币":
                    existingBag.GoldCoinsNum += task.RewardNumber;
                    break;
                case "石材":
                    existingBag.MasonryNum += task.RewardNumber;
                    break;
                case "灵气":
                    existingBag.SpiritNum += task.RewardNumber;
                    break;
                case "爱心":
                    existingBag.LoveNum += task.RewardNumber;
                    break;
                // 可以根据实际奖励类型继续扩展
                default:
                    // 其他奖励类型的处理逻辑
                    break;
            }

            // 更新背包数据到数据库，确保提供了更新条件，防止错误
            SqlSugarHelper.Db.Updateable(existingBag).Where(p => p.UId == existingBag.UId).ExecuteCommand();
        }

        private void CreateNewBagAndHandleReward(long userId, UserTask task)
        {
            // 创建新的背包数据对象
            var newBag = new Bag
            {
                UId = userId,
                GoldCoinsNum = 0,
                MasonryNum = 0,
                SpiritNum = 0,
                LoveNum = 0,
            };

            // 根据任务的奖励类型和数量初始化
            switch (task.Reward)
            {
                case "金币":
                    newBag.GoldCoinsNum = task.RewardNumber;
                    break;
                case "石材":
                    newBag.MasonryNum = task.RewardNumber;
                    break;
                case "灵气":
                    newBag.SpiritNum = task.RewardNumber;
                    break;
                case "爱心":
                    newBag.LoveNum = task.RewardNumber;
                    break;
                // 可以根据实际奖励类型继续扩展
                default:
                    // 其他奖励类型的初始化逻辑
                    break;
            }

            // 插入新的背包数据到数据库
            SqlSugarHelper.Db.Insertable(newBag).ExecuteCommand();
        }

    }
}
