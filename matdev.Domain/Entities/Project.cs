using matdev.Domain.Entities.Lab;
using System;
using System.Collections.Generic;
using System.Text;

namespace matdev.Domain.Entities
{
    public class Project
    {
        public int ProjectID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public int? IssueTypeID { get; set; }
        public int? WorkpackageID { get; set; }
        public int? TopicID { get; set; }
        public int? ProjectStatusID { get; set; }
        public int? ResponsibleID { get; set; }
        public int? SupportID { get; set; }
        public int? PriorityID { get; set; }
        public int? CreatedByID { get; set; }

        public IssueType IssueType { get; set; }
        public Workpackage Workpackage { get; set; }
        public Topic Topic { get; set; }
        public Status ProjectStatus {  get; set; }
        public Priority Priority { get; set; }
        public User Responsible { get; set; }
        public User Support { get; set; }
        public User CreatedBy { get; set; }

        public ICollection<_Task> Tasks { get; set; }
        public ICollection<LabOrderAssigment> LabOrderAssigments { get; set; }
    }
}
