using System;
using System.Collections.Generic;
using System.Text;

namespace matdev.Domain.Entities
{
    public class TaskDependency
    {
        public int TaskDependencyID { get; set; }
        public int PredecessorID { get; set; }
        public Task Predecessor { get; set; }
        public int SuccessorID { get; set; }
        public Task Successor { get; set; }
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
