using matdev.Domain.Entities.BudgetEntities;
using matdev.Domain.Interfaces;
using matdev.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace matdev.Infrastructure.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly ApplicationDbContext _db;

    public BudgetRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<BudgetPlan?> GetBudgetPlanByProjectAsync(int projectId)
    {
        return await _db.BudgetPlans
            .Include(p => p.Lines)
                .ThenInclude(l => l.Category)
            .Include(p => p.Expenditures)
                .ThenInclude(e => e.BudgetCategory)
            .Include(p => p.Expenditures)
                .ThenInclude(e => e.Task)
            .FirstOrDefaultAsync(p => p.ProjectID == projectId);
    }

    public async Task<IReadOnlyList<BudgetPlan>> GetAllBudgetPlansAsync()
    {
        return await _db.BudgetPlans
            .Include(p => p.Expenditures)
            .ToListAsync();
    }

    public async Task AddBudgetPlanAsync(BudgetPlan plan)
    {
        _db.BudgetPlans.Add(plan);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateBudgetPlanAsync(BudgetPlan plan)
    {
        _db.BudgetPlans.Update(plan);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<BudgetCategory>> GetBudgetCategoriesAsync()
    {
        return await _db.BudgetCategories.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<BudgetCategory?> GetBudgetCategoryByIdAsync(int categoryId)
    {
        return await _db.BudgetCategories.FirstOrDefaultAsync(c => c.CategoryID == categoryId);
    }

    public async Task AddBudgetCategoryAsync(BudgetCategory category)
    {
        _db.BudgetCategories.Add(category);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<BudgetPlanLine>> GetBudgetPlanLinesAsync(int planId)
    {
        return await _db.BudgetPlanLines
            .Include(l => l.Category)
            .Where(l => l.PlanID == planId)
            .OrderBy(l => l.Category.Name)
            .ToListAsync();
    }

    public async Task ReplaceBudgetPlanLinesAsync(int planId, IReadOnlyList<BudgetPlanLine> lines)
    {
        var existing = await _db.BudgetPlanLines.Where(l => l.PlanID == planId).ToListAsync();
        _db.BudgetPlanLines.RemoveRange(existing);
        if (lines.Count > 0)
            await _db.BudgetPlanLines.AddRangeAsync(lines);
        await _db.SaveChangesAsync();
    }

    public async Task AddExpenditureAsync(BudgetExpenditure expenditure)
    {
        _db.BudgetExpenditures.Add(expenditure);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateExpenditureAsync(BudgetExpenditure expenditure)
    {
        _db.BudgetExpenditures.Update(expenditure);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> TaskExistsInProjectAsync(int projectId, int taskId) =>
        await _db.Tasks.AnyAsync(t => t.ProjectID == projectId && t.TaskID == taskId);

    public async Task<decimal> GetTaskExpenditureSumAsync(int projectId, int taskId) =>
        await _db.BudgetExpenditures
            .Where(e => e.TaskID == taskId && e.BudgetPlan.ProjectID == projectId)
            .SumAsync(e => e.Amount);

    public async Task<IReadOnlyList<BudgetExpenditure>> GetExpendituresForTaskAsync(int projectId, int taskId) =>
        await _db.BudgetExpenditures
            .Include(e => e.BudgetCategory)
            .Where(e => e.TaskID == taskId && e.BudgetPlan.ProjectID == projectId)
            .OrderByDescending(e => e.TransactionDate)
            .ToListAsync();

    public async Task<BudgetExpenditure?> GetExpenditureAsync(int expenditureId)
    {
        return await _db.BudgetExpenditures
            .Include(e => e.BudgetCategory)
            .FirstOrDefaultAsync(e => e.ExpenditureID == expenditureId);
    }

    public async Task DeleteExpenditureAsync(BudgetExpenditure expenditure)
    {
        _db.BudgetExpenditures.Remove(expenditure);
        await _db.SaveChangesAsync();
    }
}
