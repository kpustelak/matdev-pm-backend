using System;
using System.Collections.Generic;
using matdev.Domain.Entities;

namespace matdev.Domain.Entities.BudgetEntities
{
    public class BudgetPlan
    {
        public int PlanID { get; set; }
        public int ProjectID { get; set; }
        public Project Project { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime LastUpdated { get; set; }
        public ICollection<BudgetExpenditure> Expenditures { get; set; } = new List<BudgetExpenditure>();
        public ICollection<BudgetPlanLine> Lines { get; set; } = new List<BudgetPlanLine>();
    }
}
