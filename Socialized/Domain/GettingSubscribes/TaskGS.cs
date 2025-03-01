﻿using Domain.InstagramAccounts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.GettingSubscribes
{
    public partial class TaskGS : BaseEntity
    {
        public TaskGS()
        {
            Data = new HashSet<TaskData>();
        }
        [ForeignKey("Account")]
        public long AccountId { get; set; }
        public sbyte Type { get; set; }
        public sbyte Subtype { get; set; }
        public long LastDoneAt { get; set; }
        public bool Updated { get; set; }
        public bool Running { get; set; }
        public bool Stopped { get; set; }
        public long NextTaskData { get; set; }
        public bool Deleted { get; set; }
        public virtual IGAccount Account { get; set; }
        public virtual TaskFilter Filter { get; set; }
        public virtual TaskOption Option { get; set; }
        public virtual ICollection<TaskData>  Data { get; set; }
    }
}