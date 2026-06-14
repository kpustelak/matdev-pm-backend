using matdev.Domain.Entities;

namespace matdev.Domain.Interfaces;

public interface IProjectRiskRepository
{
    Task<IReadOnlyList<ProjectRisk>> GetByProjectAsync(int projectId);
    Task<ProjectRisk?> GetByIdAsync(int riskId);
    Task<ProjectRisk> AddAsync(ProjectRisk risk);
    Task UpdateAsync(ProjectRisk risk);
    Task DeleteAsync(ProjectRisk risk);
}
