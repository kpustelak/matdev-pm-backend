using matdev.Application.DTOs.Budget;

namespace matdev.Application.Interfaces;

public interface IBudgetService
{
    Task<GetProjectBudgetDTO?> GetProjectBudgetAsync(int projectId);
    Task<GetProjectBudgetDTO> CreateBudgetPlanAsync(int projectId, CreateBudgetPlanDTO dto);
    Task<IReadOnlyList<BudgetCategoryDTO>> GetBudgetCategoriesAsync();
    Task<BudgetCategoryDTO> CreateBudgetCategoryAsync(CreateBudgetCategoryDTO dto);
    Task<IReadOnlyList<BudgetPlanLineDTO>> GetBudgetLinesAsync(int projectId);
    Task<IReadOnlyList<BudgetPlanLineDTO>> ReplaceBudgetLinesAsync(int projectId, UpdateBudgetLinesDTO dto);
    Task<GetProjectBudgetDTO?> UpdateBudgetPlanAsync(int projectId, UpdateBudgetPlanDTO dto);
    Task<GetProjectBudgetDTO?> AddExpenditureAsync(int projectId, CreateExpenditureDTO dto);
    Task<GetProjectBudgetDTO?> UpdateExpenditureAsync(int projectId, int expenditureId, UpdateExpenditureDTO dto);
    Task<GetProjectBudgetDTO?> DeleteExpenditureAsync(int projectId, int expenditureId);
}
