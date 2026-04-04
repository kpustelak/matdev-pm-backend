using System;
using System.Collections.Generic;
using System.Text;

namespace matdev.Domain.Entities
{
    public class TaskAssigment
    {
        public int TaskAssigmentID { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }
        public int TaskID { get; set; }
        public _Task Task { get; set; }

    }
}
