using matdev.Domain.Entities.TaskEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace matdev.Domain.Entities
{
    public class User
    {
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public ICollection<TaskAssignment> TaskAssigments { get; set; }
        public ICollection<TimeEntry> TimeEntries { get; set; }

    }
}
