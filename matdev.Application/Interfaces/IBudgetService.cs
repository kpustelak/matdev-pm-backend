using matdev.Application.DTOs.Budget;

namespace matdev.Application.Interfaces;

public interface IBudgetService
{
    Task<GetProjectBudgetDTO?> GetProjectBudgetAsync(int projectId);
    Task<IReadOnlyList<BudgetCategoryDTO>> GetBudgetCategoriesAsync();
    Task<GetProjectBudgetDTO?> UpdateBudgetPlanAsync(int projectId, UpdateBudgetPlanDTO dto);
    Task<GetProjectBudgetDTO?> AddExpenditureAsync(int projectId, CreateExpenditureDTO dto);
    Task<GetProjectBudgetDTO?> DeleteExpenditureAsync(int projectId, int expenditureId);
}
