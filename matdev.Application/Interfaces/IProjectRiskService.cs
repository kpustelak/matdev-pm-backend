using matdev.Application.DTOs.Risk;

namespace matdev.Application.Interfaces;

public interface IProjectRiskService
{
    Task<IReadOnlyList<GetProjectRiskDTO>> GetByProjectAsync(int projectId);
    Task<GetProjectRiskDTO> CreateAsync(int projectId, CreateProjectRiskDTO dto);
    Task<GetProjectRiskDTO> ResolveAsync(int projectId, int riskId);
    Task DeleteAsync(int projectId, int riskId);
}
