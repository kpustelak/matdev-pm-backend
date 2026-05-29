using matdev.Application.DTOs.Budget;
using matdev.Application.Interfaces;
using matdev.Domain.Entities.BudgetEntities;
using matdev.Domain.Interfaces;

namespace matdev.Application.Services;

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _repository;

    public BudgetService(IBudgetRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetProjectBudgetDTO?> GetProjectBudgetAsync(int projectId)
    {
        var plan = await _repository.GetBudgetPlanByProjectAsync(projectId);
        return plan is null ? null : MapToDTO(plan);
    }

    public async Task<IReadOnlyList<BudgetCategoryDTO>> GetBudgetCategoriesAsync()
    {
        var cats = await _repository.GetBudgetCategoriesAsync();
        return cats.Select(c => new BudgetCategoryDTO(c.CategoryID, c.Name)).ToList();
    }

    public async Task<GetProjectBudgetDTO?> UpdateBudgetPlanAsync(int projectId, UpdateBudgetPlanDTO dto)
    {
        var plan = await _repository.GetBudgetPlanByProjectAsync(projectId);
        if (plan is null) return null;

        plan.Name = dto.Name;
        plan.Amount = dto.Amount;
        plan.LastUpdated = DateTime.UtcNow;
        await _repository.UpdateBudgetPlanAsync(plan);

        return MapToDTO(plan);
    }

    public async Task<GetProjectBudgetDTO?> AddExpenditureAsync(int projectId, CreateExpenditureDTO dto)
    {
        var plan = await _repository.GetBudgetPlanByProjectAsync(projectId);
        if (plan is null) return null;

        var expenditure = new BudgetExpenditure
        {
            BudgetPlanID = plan.PlanID,
            BudgetCategoryID = dto.CategoryId,
            Amount = dto.Amount,
            TransactionDate = dto.TransactionDate,
            Description = dto.Description,
            Field = dto.Field,
        };
        await _repository.AddExpenditureAsync(expenditure);

        var refreshed = await _repository.GetBudgetPlanByProjectAsync(projectId);
        return refreshed is null ? null : MapToDTO(refreshed);
    }

    public async Task<GetProjectBudgetDTO?> DeleteExpenditureAsync(int projectId, int expenditureId)
    {
        var expenditure = await _repository.GetExpenditureAsync(expenditureId);
        // Verify the expenditure belongs to this project before deleting
        if (expenditure is not null)
        {
            var ownerPlan = await _repository.GetBudgetPlanByProjectAsync(projectId);
            if (ownerPlan is null || expenditure.BudgetPlanID != ownerPlan.PlanID)
                throw new KeyNotFoundException($"Expenditure {expenditureId} not found in project {projectId}.");
            await _repository.DeleteExpenditureAsync(expenditure);
        }

        var plan = await _repository.GetBudgetPlanByProjectAsync(projectId);
        return plan is null ? null : MapToDTO(plan);
    }

    private static GetProjectBudgetDTO MapToDTO(BudgetPlan plan)
    {
        var categories = plan.Expenditures
            .GroupBy(e => e.BudgetCategory)
            .Select(g => new BudgetCategorySpendDTO(
                g.Key.CategoryID,
                g.Key.Name,
                g.Sum(e => e.Amount),
                g.Select(e => new BudgetExpenditureDTO(
                    e.ExpenditureID,
                    e.BudgetCategory.Name,
                    e.Amount,
                    e.TransactionDate,
                    e.Description,
                    e.Field))
                 .OrderByDescending(e => e.TransactionDate)
                 .ToList()))
            .OrderByDescending(c => c.TotalSpent)
            .ToList();

        var totalSpent = categories.Sum(c => c.TotalSpent);
        var freeBudget = plan.Amount - totalSpent;

        return new GetProjectBudgetDTO(
            plan.PlanID,
            plan.Name,
            plan.Amount,
            totalSpent,
            freeBudget < 0 ? 0 : freeBudget,
            categories);
    }
}
