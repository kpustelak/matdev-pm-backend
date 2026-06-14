using matdev.Domain.Entities;
using matdev.Domain.Interfaces;
using matdev.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace matdev.Infrastructure.Repositories;

public class ProjectRiskRepository : IProjectRiskRepository
{
    private readonly ApplicationDbContext _db;

    public ProjectRiskRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ProjectRisk>> GetByProjectAsync(int projectId)
    {
        return await _db.ProjectRisks
            .Where(r => r.ProjectID == projectId)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<ProjectRisk?> GetByIdAsync(int riskId)
    {
        return await _db.ProjectRisks.FindAsync(riskId);
    }

    public async Task<ProjectRisk> AddAsync(ProjectRisk risk)
    {
        _db.ProjectRisks.Add(risk);
        await _db.SaveChangesAsync();
        return risk;
    }

    public async Task UpdateAsync(ProjectRisk risk)
    {
        _db.ProjectRisks.Update(risk);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ProjectRisk risk)
    {
        _db.ProjectRisks.Remove(risk);
        await _db.SaveChangesAsync();
    }
}
