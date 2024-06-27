using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.DBModel
{
    [Serializable]
    public class TaskChange
    {
        public long UId { get; set; }
        public string description { get; set; }
        public bool isCompleted { get; set; }
    }
    [Serializable]
    public class TaskDatas
    {
        public long UId { get; set; }
        public int DayKill { get; set; }
        public int DayMatch { get; set; }
        public long AllKill { get; set; }
        public long AllDays { get; set; }
    }
    [Serializable]
    public class ReqChangeTaskDatas
    {
        public long UId { get; set;}
        public String TaskType { get; set; }
    }
}
