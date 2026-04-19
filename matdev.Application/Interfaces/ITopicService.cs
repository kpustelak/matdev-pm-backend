using matdev.Application.DTOs.Topic;

namespace matdev.Application.Interfaces;

public interface ITopicService
{
    Task<GetTopicDTO> GetByIdAsync(int id);
    Task<IEnumerable<GetTopicDTO>> GetAllAsync();
    Task<GetTopicDTO> CreateAsync(CreateTopicDTO dto);
    Task<GetTopicDTO> UpdateAsync(EditTopicDTO dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<GetTopicDTO>> GetByPhraseAsync(string phrase);
}
