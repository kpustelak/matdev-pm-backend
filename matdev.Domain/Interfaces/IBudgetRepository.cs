using matdev.Domain.Entities.BudgetEntities;

namespace matdev.Domain.Interfaces;

public interface IBudgetRepository
{
    Task<BudgetPlan?> GetBudgetPlanByProjectAsync(int projectId);
    Task<IReadOnlyList<BudgetPlan>> GetAllBudgetPlansAsync();
    Task UpdateBudgetPlanAsync(BudgetPlan plan);
    Task<IReadOnlyList<BudgetCategory>> GetBudgetCategoriesAsync();
    Task AddExpenditureAsync(BudgetExpenditure expenditure);
    Task<BudgetExpenditure?> GetExpenditureAsync(int expenditureId);
    Task DeleteExpenditureAsync(BudgetExpenditure expenditure);
}
