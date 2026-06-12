using System.Collections.Generic;
using System.Linq;
using matdev.Application.DTOs.Budget;
using matdev.Application.Interfaces;
using matdev.Domain.Entities.BudgetEntities;
using matdev.Domain.Interfaces;

namespace matdev.Application.Services;

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _repository;
    private readonly IProjectRepository _projectRepository;

    public BudgetService(IBudgetRepository repository, IProjectRepository projectRepository)
    {
        _repository = repository;
        _projectRepository = projectRepository;
    }

    public async Task<GetProjectBudgetDTO?> GetProjectBudgetAsync(int projectId)
    {
        var plan = await _repository.GetBudgetPlanByProjectAsync(projectId);
        return plan is null ? null : MapToDTO(plan);
    }

    public async Task<GetProjectBudgetDTO> CreateBudgetPlanAsync(int projectId, CreateBudgetPlanDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Plan name is required.");

        if (dto.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project is null)
            throw new KeyNotFoundException($"Project {projectId} not found.");

        var existing = await _repository.GetBudgetPlanByProjectAsync(projectId);
        if (existing is not null)
            throw new InvalidOperationException("This project already has a budget plan.");

        var plan = new BudgetPlan
        {
            ProjectID = projectId,
            Name = dto.Name.Trim(),
            Amount = dto.Amount,
            LastUpdated = DateTime.UtcNow,
            Expenditures = new List<BudgetExpenditure>(),
            Lines = new List<BudgetPlanLine>(),
        };
        await _repository.AddBudgetPlanAsync(plan);

        var created = await _repository.GetBudgetPlanByProjectAsync(projectId);
        return MapToDTO(created!);
    }

    public async Task<IReadOnlyList<BudgetCategoryDTO>> GetBudgetCategoriesAsync()
    {
        var cats = await _repository.GetBudgetCategoriesAsync();
        return cats.Select(c => new BudgetCategoryDTO(
            c.CategoryID,
            c.Name,
            NormalizeCategoryDefaultThreshold(c.DefaultAlertThreshold))).ToList();
    }

    public async Task<BudgetCategoryDTO> CreateBudgetCategoryAsync(CreateBudgetCategoryDTO dto)
    {
        var name = dto.Name?.Trim() ?? string.Empty;
        if (name.Length == 0)
            throw new ArgumentException("Category name is required.");

        var all = await _repository.GetBudgetCategoriesAsync();
        if (all.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Category '{name}' already exists.");

        var threshold = dto.DefaultAlertThresholdPercent is >= 1 and <= 100
            ? dto.DefaultAlertThresholdPercent.Value
            : 80;

        var category = new BudgetCategory
        {
            Name = name,
            DefaultAlertThreshold = threshold,
            Expenditures = new List<BudgetExpenditure>(),
        };
        await _repository.AddBudgetCategoryAsync(category);

        return new BudgetCategoryDTO(category.CategoryID, category.Name, threshold);
    }

    public async Task<IReadOnlyList<BudgetPlanLineDTO>> GetBudgetLinesAsync(int projectId)
    {
        var plan = await _repository.GetBudgetPlanByProjectAsync(projectId);
        if (plan is null)
            return Array.Empty<BudgetPlanLineDTO>();

        return (plan.Lines ?? Array.Empty<BudgetPlanLine>())
            .Select(l => new BudgetPlanLineDTO(
                l.CategoryID,
                l.Category?.Name,
                l.AllocatedAmount,
                l.AlertThresholdPercent))
            .ToList();
    }

    public async Task<IReadOnlyList<BudgetPlanLineDTO>> ReplaceBudgetLinesAsync(int projectId, UpdateBudgetLinesDTO dto)
    {
        var plan = await _repository.GetBudgetPlanByProjectAsync(projectId);
        if (plan is null)
            throw new KeyNotFoundException("No budget plan found for this project.");

        var items = dto.Lines ?? Array.Empty<UpdateBudgetLineItemDTO>();
        if (items.Any(l => l.AllocatedAmount < 0))
            throw new ArgumentException("Allocated amounts cannot be negative.");

        var categoryIds = items.Select(l => l.CategoryId).ToList();
        if (categoryIds.Distinct().Count() != categoryIds.Count)
            throw new ArgumentException("Duplicate category in allocation lines.");

        foreach (var categoryId in categoryIds)
        {
            if (await _repository.GetBudgetCategoryByIdAsync(categoryId) is null)
                throw new KeyNotFoundException($"Budget category {categoryId} not found.");
        }

        var totalAllocated = items.Sum(l => l.AllocatedAmount);
        if (totalAllocated > plan.Amount)
            throw new ArgumentException(
                $"Sum of allocations ({totalAllocated}) exceeds plan total ({plan.Amount}).");

        var entities = items
            .Where(l => l.AllocatedAmount > 0 || l.AlertThresholdPercent is not null)
            .Select(l => new BudgetPlanLine
            {
                PlanID = plan.PlanID,
                CategoryID = l.CategoryId,
                AllocatedAmount = l.AllocatedAmount,
                AlertThresholdPercent = l.AlertThresholdPercent is >= 1 and <= 100
                    ? l.AlertThresholdPercent
                    : null,
            })
            .ToList();

        await _repository.ReplaceBudgetPlanLinesAsync(plan.PlanID, entities);
        plan.LastUpdated = DateTime.UtcNow;
        await _repository.UpdateBudgetPlanAsync(plan);

        return await GetBudgetLinesAsync(projectId);
    }

    public async Task<GetProjectBudgetDTO?> UpdateBudgetPlanAsync(int projectId, UpdateBudgetPlanDTO dto)
    {
        var plan = await _repository.GetBudgetPlanByProjectAsync(projectId);
        if (plan is null) return null;

        plan.Name = dto.Name;
        plan.Amount = dto.Amount;
        plan.LastUpdated = DateTime.UtcNow;
        await _repository.UpdateBudgetPlanAsync(plan);

        var refreshed = await _repository.GetBudgetPlanByProjectAsync(projectId);
        return refreshed is null ? null : MapToDTO(refreshed);
    }

    public async Task<GetProjectBudgetDTO?> AddExpenditureAsync(int projectId, CreateExpenditureDTO dto)
    {
        var plan = await _repository.GetBudgetPlanByProjectAsync(projectId);
        if (plan is null) return null;

        if (dto.TaskId is int taskId && !await _repository.TaskExistsInProjectAsync(projectId, taskId))
            throw new KeyNotFoundException($"Task {taskId} not found in project {projectId}.");

        var expenditure = new BudgetExpenditure
        {
            BudgetPlanID = plan.PlanID,
            BudgetCategoryID = dto.CategoryId,
            TaskID = dto.TaskId,
            Amount = dto.Amount,
            TransactionDate = dto.TransactionDate,
            Description = dto.Description,
            Field = dto.Field,
        };
        await _repository.AddExpenditureAsync(expenditure);

        var refreshed = await _repository.GetBudgetPlanByProjectAsync(projectId);
        return refreshed is null ? null : MapToDTO(refreshed);
    }

    public async Task<GetProjectBudgetDTO?> UpdateExpenditureAsync(int projectId, int expenditureId, UpdateExpenditureDTO dto)
    {
        var expenditure = await _repository.GetExpenditureAsync(expenditureId)
            ?? throw new KeyNotFoundException($"Expenditure {expenditureId} not found.");

        var ownerPlan = await _repository.GetBudgetPlanByProjectAsync(projectId)
            ?? throw new KeyNotFoundException("No budget plan found for this project.");
        if (expenditure.BudgetPlanID != ownerPlan.PlanID)
            throw new KeyNotFoundException($"Expenditure {expenditureId} not found in project {projectId}.");

        if (await _repository.GetBudgetCategoryByIdAsync(dto.CategoryId) is null)
            throw new KeyNotFoundException($"Budget category {dto.CategoryId} not found.");

        if (dto.TaskId is int taskId && !await _repository.TaskExistsInProjectAsync(projectId, taskId))
            throw new KeyNotFoundException($"Task {taskId} not found in project {projectId}.");

        expenditure.BudgetCategoryID = dto.CategoryId;
        expenditure.Amount = dto.Amount;
        expenditure.TransactionDate = dto.TransactionDate;
        expenditure.Description = dto.Description;
        expenditure.Field = dto.Field;
        expenditure.TaskID = dto.TaskId;
        await _repository.UpdateExpenditureAsync(expenditure);

        var refreshed = await _repository.GetBudgetPlanByProjectAsync(projectId);
        return refreshed is null ? null : MapToDTO(refreshed);
    }

    public async Task<GetProjectBudgetDTO?> DeleteExpenditureAsync(int projectId, int expenditureId)
    {
        var expenditure = await _repository.GetExpenditureAsync(expenditureId);
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

    private static int? NormalizeCategoryDefaultThreshold(decimal value)
    {
        if (value <= 0) return null;
        if (value > 100) return 80;
        return (int)value;
    }

    private static GetProjectBudgetDTO MapToDTO(BudgetPlan plan)
    {
        var lineByCategory = (plan.Lines ?? Array.Empty<BudgetPlanLine>())
            .ToDictionary(l => l.CategoryID);

        var spendGroups = plan.Expenditures
            .GroupBy(e => e.BudgetCategory)
            .ToDictionary(g => g.Key.CategoryID, g => g);

        var categoryIds = spendGroups.Keys.Union(lineByCategory.Keys).Distinct();

        var categories = categoryIds.Select(categoryId =>
        {
            spendGroups.TryGetValue(categoryId, out var spendGroup);
            lineByCategory.TryGetValue(categoryId, out var line);

            var categoryName = spendGroup?.Key.Name ?? line?.Category?.Name ?? "Unknown";
            var totalSpent = spendGroup?.Sum(e => e.Amount) ?? 0m;
            var allocated = line?.AllocatedAmount;
            decimal? remaining = allocated is not null ? allocated - totalSpent : null;
            decimal? utilization = allocated is > 0
                ? Math.Round(totalSpent / allocated.Value * 100m, 1)
                : totalSpent > 0 ? 100m : null;

            var expenditures = (spendGroup?
                .Select(e => new BudgetExpenditureDTO(
                    e.ExpenditureID,
                    e.BudgetCategoryID,
                    e.BudgetCategory.Name,
                    e.Amount,
                    e.TransactionDate,
                    e.Description,
                    e.Field,
                    e.TaskID,
                    e.Task?.Name))
                .OrderByDescending(e => e.TransactionDate)
                .ToList()) ?? new List<BudgetExpenditureDTO>();

            return new BudgetCategorySpendDTO(
                categoryId,
                categoryName,
                totalSpent,
                allocated,
                remaining,
                utilization,
                expenditures);
        })
        .OrderByDescending(c => c.TotalSpent)
        .ThenBy(c => c.CategoryName)
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
