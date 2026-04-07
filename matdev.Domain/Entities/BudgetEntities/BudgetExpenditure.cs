using System;
using System.Collections.Generic;
using System.Text;

namespace matdev.Domain.Entities.BudgetEntities
{
    public class BudgetExpenditure
    {
        public int ExpenditureID { get; set; }
        public int BudgetPlanID { get; set; }
        public BudgetPlan BudgetPlan { get; set; }
        public int BudgetCategoryID { get; set; }
        public BudgetCategory BudgetCategory { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public string Field { get; set; }
    }
}
