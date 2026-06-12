using matdev.Domain.Entities.BudgetEntities;

namespace matdev.Domain.Interfaces;

public interface IBudgetRepository
{
    Task<BudgetPlan?> GetBudgetPlanByProjectAsync(int projectId);
    Task<IReadOnlyList<BudgetPlan>> GetAllBudgetPlansAsync();
    Task AddBudgetPlanAsync(BudgetPlan plan);
    Task UpdateBudgetPlanAsync(BudgetPlan plan);
    Task<IReadOnlyList<BudgetCategory>> GetBudgetCategoriesAsync();
    Task<BudgetCategory?> GetBudgetCategoryByIdAsync(int categoryId);
    Task AddBudgetCategoryAsync(BudgetCategory category);
    Task<IReadOnlyList<BudgetPlanLine>> GetBudgetPlanLinesAsync(int planId);
    Task ReplaceBudgetPlanLinesAsync(int planId, IReadOnlyList<BudgetPlanLine> lines);
    Task AddExpenditureAsync(BudgetExpenditure expenditure);
    Task UpdateExpenditureAsync(BudgetExpenditure expenditure);
    Task<BudgetExpenditure?> GetExpenditureAsync(int expenditureId);
    Task DeleteExpenditureAsync(BudgetExpenditure expenditure);
    Task<bool> TaskExistsInProjectAsync(int projectId, int taskId);
    Task<decimal> GetTaskExpenditureSumAsync(int projectId, int taskId);
    Task<IReadOnlyList<BudgetExpenditure>> GetExpendituresForTaskAsync(int projectId, int taskId);
}
