using System;
using System.Collections.Generic;
using System.Text;
using matdev.Domain.Entities;

namespace matdev.Domain.Entities.TaskEntities
{
    public class TaskAssignment
    {
        public int TaskAssignmentID { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }
        public int TaskID { get; set; }
        public _Task Task { get; set; }

    }
}
