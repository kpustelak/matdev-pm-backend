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
            .Include(p => p.Expenditures)
                .ThenInclude(e => e.BudgetCategory)
            .FirstOrDefaultAsync(p => p.ProjectID == projectId);
    }

    public async Task<IReadOnlyList<BudgetPlan>> GetAllBudgetPlansAsync()
    {
        return await _db.BudgetPlans
            .Include(p => p.Expenditures)
            .ToListAsync();
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

    public async Task AddExpenditureAsync(BudgetExpenditure expenditure)
    {
        _db.BudgetExpenditures.Add(expenditure);
        await _db.SaveChangesAsync();
    }

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
