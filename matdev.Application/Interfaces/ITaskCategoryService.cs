using matdev.Application.DTOs.TaskCategory;

namespace matdev.Application.Interfaces;

public interface ITaskCategoryService
{
    Task<GetTaskCategoryDTO> GetByIdAsync(int id);
    Task<IEnumerable<GetTaskCategoryDTO>> GetAllAsync();
    Task<GetTaskCategoryDTO> CreateAsync(CreateTaskCategoryDTO dto);
    Task<GetTaskCategoryDTO> UpdateAsync(EditTaskCategoryDTO dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<GetTaskCategoryDTO>> GetByPhraseAsync(string phrase);
}
