using AutoMapper;
using matdev.Application.DTOs.TaskCategory;
using matdev.Application.Interfaces;
using matdev.Domain.Entities.TaskEntities;
using matdev.Domain.Interfaces;

namespace matdev.Application.Services;

public class TaskCategoryService : ITaskCategoryService
{
    private readonly ITaskCategoryRepository _repository;
    private readonly IMapper _mapper;

    public TaskCategoryService(ITaskCategoryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<GetTaskCategoryDTO> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            throw new KeyNotFoundException($"Task category with ID {id} not found.");
        return _mapper.Map<GetTaskCategoryDTO>(entity);
    }

    public async Task<IEnumerable<GetTaskCategoryDTO>> GetAllAsync()
    {
        var list = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<GetTaskCategoryDTO>>(list);
    }

    public async Task<GetTaskCategoryDTO> CreateAsync(CreateTaskCategoryDTO dto)
    {
        var entity = await _repository.AddAsync(_mapper.Map<TaskCategory>(dto));
        return _mapper.Map<GetTaskCategoryDTO>(entity);
    }

    public async Task<GetTaskCategoryDTO> UpdateAsync(EditTaskCategoryDTO dto)
    {
        var entity = await _repository.GetByIdAsync(dto.TaskCategoryId);
        if (entity is null)
            throw new KeyNotFoundException($"Task category with ID {dto.TaskCategoryId} not found.");
        if (dto.Name is null)
            throw new ArgumentException("Name cannot be null.");
        entity.Name = dto.Name;
        await _repository.UpdateAsync(entity);
        return _mapper.Map<GetTaskCategoryDTO>(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            throw new KeyNotFoundException($"Task category with ID {id} not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task<IEnumerable<GetTaskCategoryDTO>> GetByPhraseAsync(string phrase)
    {
        var matches = await _repository.GetByPhraseAsync(phrase);
        return matches.Any()
            ? _mapper.Map<IEnumerable<GetTaskCategoryDTO>>(matches)
            : throw new KeyNotFoundException($"No task categories found containing the phrase '{phrase}'.");
    }
}
