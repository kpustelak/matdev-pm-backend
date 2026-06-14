using System;
using System.Collections.Generic;
using System.Text;
using matdev.Domain.Entities;

namespace matdev.Domain.Entities.TaskEntities
{
    public class TaskDependency
    {
        public int TaskDependencyID { get; set; }
        public int PredecessorID { get; set; }
        public _Task Predecessor { get; set; }
        public int SuccessorID { get; set; }
        public _Task Successor { get; set; }
        public DependencyType DependencyType { get; set; }
        public int LagTime { get; set; }
    }

    public enum DependencyType
    {
        FinishToStart,
        StartToStart,
        FinishToFinish,
        StartToFinish
    }
}
