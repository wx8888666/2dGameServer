using SqlSugar;

namespace _2DSurviveGameServer._01Common
{
    public class Friendrequest
    {
        [Serializable]
        public class FriendRequests
        {
            [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
            public long Id { get; set; }

            public long SenderId { get; set; }

            public long ReceiverId { get; set; }

            public string Status { get; set; } // 'pending', 'accepted', 'rejected'

            // 不指定默认值，在数据库中使用 DEFAULT CURRENT_TIMESTAMP
            [SugarColumn(ColumnDataType = "datetime", IsNullable = false)]
            public DateTime Timestamp { get; set; }
        }

        [Serializable]
        public class Notifications
        {
            [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
            public long Id { get; set; }

            public long UserId { get; set; }

            public string Message { get; set; }

            public bool IsRead { get; set; }

            // 不指定默认值，在数据库中使用 DEFAULT CURRENT_TIMESTAMP
            [SugarColumn(ColumnDataType = "datetime", IsNullable = false)]
            public DateTime Timestamp { get; set; }
        }
        [Serializable]
        public class TaskType
        {
            [SugarColumn(IsPrimaryKey = true, IsIdentity = true)] // 使用 Sugar ORM 指定自增主键
            public int Id { get; set; }

            public string Description { get; set; }
            public string Reward { get; set; }
            public int Number { get; set; }
            public int RewardNumber { get; set; }
            public string Condition { get; set; }


            //public override string ToString()
            //{
            //    return $"Id: {Id}, Description: {Description}, Reward: {Reward} {RewardNumber}, Condition: {Condition}";
            //}
        }

    }
}
