using matdev.Application.DTOs.Project;

namespace matdev.Application.Interfaces;

public interface IProjectService
{
    Task<GetProjectDTO> GetByIdAsync(int id);
    Task<IEnumerable<GetProjectDTO>> GetAllAsync();
    Task<GetProjectDTO> CreateAsync(CreateProjectDTO dto);
    Task<GetProjectDTO> UpdateAsync(EditProjectDTO dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<GetProjectDTO>> GetByPhraseAsync(string s);
}
