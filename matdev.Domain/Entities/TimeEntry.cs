using matdev.Domain.Entities.TaskEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace matdev.Domain.Entities
{
    public class TimeEntry
    {
        public int TimeEntryID { get; set; }
        public int TaskID { get; set; }
        public _Task Task { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }
        public float Hours { get; set; }
        public DateTime EntryDate { get; set; }
    }
}
