using matdev.Application.DTOs.Workpackage;

namespace matdev.Application.Interfaces;

public interface IWorkpackageService
{
    Task<GetWorkpackageDTO> GetByIdAsync(int id);
    Task<IEnumerable<GetWorkpackageDTO>> GetAllAsync();
    Task<GetWorkpackageDTO> CreateAsync(CreateWorkpackageDTO dto);
    Task<GetWorkpackageDTO> UpdateAsync(EditWorkpackageDTO dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<GetWorkpackageDTO>> GetByPhraseAsync(string phrase);
}
