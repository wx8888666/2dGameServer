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
    }
}
