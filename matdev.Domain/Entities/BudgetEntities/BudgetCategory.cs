using System;
using System.Collections.Generic;
using System.Text;

namespace matdev.Domain.Entities.BudgetEntities
{
    public class BudgetCategory
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public decimal DefaultAlertThreshold { get; set; }
        public ICollection<BudgetExpenditure> Expenditures { get; set; }
    }
}
