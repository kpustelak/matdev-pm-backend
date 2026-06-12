namespace matdev.Domain.Entities.BudgetEntities;

public class BudgetPlanLine
{
    public int LineID { get; set; }
    public int PlanID { get; set; }
    public BudgetPlan Plan { get; set; } = null!;
    public int CategoryID { get; set; }
    public BudgetCategory Category { get; set; } = null!;
    public decimal AllocatedAmount { get; set; }
    public int? AlertThresholdPercent { get; set; }
}
