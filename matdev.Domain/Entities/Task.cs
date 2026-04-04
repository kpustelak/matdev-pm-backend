using System;
using System.Collections.Generic;
using System.Text;

namespace matdev.Domain.Entities
{
    public class _Task
    {
        public int TaskID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsMilestone { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Progress { get; set; }
        public int SortOrder { get; set; }

        public int ProjectID { get; set; }
        public int? StatusID { get; set; }
        public int? PriorityID { get; set; }
        public int? ParentID { get; set; }
        public int? RequesterID { get; set; }

        public Project Project { get; set; }
        public Status Status { get; set; }
        public Priority Priority { get; set; }
        public _Task ParentTask { get; set; }
        public User Requester { get; set; }

        public ICollection<TaskAssigment> Assigments { get; set; }
        public ICollection<TimeEntry> TimeEntries { get; set; }


    }
}
