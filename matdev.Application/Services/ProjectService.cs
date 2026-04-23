using AutoMapper;
using matdev.Application.DTOs.Project;
using matdev.Application.Interfaces;
using matdev.Domain.Entities;
using matdev.Domain.Interfaces;

namespace matdev.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;
    private readonly IMapper _mapper;

    public ProjectService(IProjectRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<GetProjectDTO> GetByIdAsync(int id)
    {
        var project = await _repository.GetByIdAsync(id);
        if (project is null)
            throw new KeyNotFoundException($"Project with id {id} was not found.");

        return MapToDto(project);
    }

    public async Task<IEnumerable<GetProjectDTO>> GetAllAsync()
    {
        var projects = await _repository.GetAllAsync();
        return projects.Select(MapToDto);
    }

    public async Task<GetProjectDTO> CreateAsync(CreateProjectDTO dto)
    {
        var project = _mapper.Map<Project>(dto);
        project.CreatedAt = DateTime.UtcNow;

        var created = await _repository.AddAsync(project);
        return MapToDto(created);
    }

    public async Task<GetProjectDTO> UpdateAsync(EditProjectDTO dto)
    {
        var existing = await _repository.GetByIdAsync(dto.ProjectId);
        if (existing is null)
            throw new KeyNotFoundException($"Project with id {dto.ProjectId} was not found.");

        if (dto.ProjectName is null
            && dto.TopicId is null
            && dto.StatusId is null
            && dto.PriorityId is null
            && dto.IssuetypeId is null
            && dto.RespPeronId is null
            && dto.SuppPersonId is null
            && dto.StartDate is null
            && dto.EndDate is null
            && dto.WorkpackageId is null
            && dto.Description is null)
            throw new ArgumentException("Send at least one field to update.");

        if (dto.ProjectName is not null)
            existing.Name = dto.ProjectName;
        if (dto.TopicId is not null)
            existing.TopicID = dto.TopicId;
        if (dto.StatusId is not null)
            existing.ProjectStatusID = dto.StatusId;
        if (dto.PriorityId is not null)
            existing.PriorityID = dto.PriorityId;
        if (dto.IssuetypeId is not null)
            existing.IssueTypeID = dto.IssuetypeId;
        if (dto.RespPeronId is not null)
            existing.ResponsibleID = dto.RespPeronId;
        if (dto.SuppPersonId is not null)
            existing.SupportID = dto.SuppPersonId;
        if (dto.StartDate is not null)
            existing.StartDate = dto.StartDate;
        if (dto.EndDate is not null)
            existing.EndDate = dto.EndDate;
        if (dto.WorkpackageId is not null)
            existing.WorkpackageID = dto.WorkpackageId;
        if (dto.Description is not null)
            existing.Description = dto.Description;

        await _repository.UpdateAsync(existing);
        return MapToDto(existing);
    }

    public async Task DeleteAsync(int id)
    {
        var project = await _repository.GetByIdAsync(id);
        if (project is null)
            throw new KeyNotFoundException($"Project with id {id} was not found.");

        await _repository.DeleteAsync(project);
    }

    public async Task<IEnumerable<GetProjectDTO>> GetByPhraseAsync(string s)
    {
        var projects = await _repository.GetByPhraseAsync(s);
        if (!projects.Any())
            throw new KeyNotFoundException("There is no project with matching data.");

        return projects.Select(MapToDto);
    }

    private static GetProjectDTO MapToDto(Project project)
    {
        return new GetProjectDTO(
            project.ProjectID,
            project.Name,
            project.Description,
            project.StartDate,
            project.EndDate,
            project.TopicID,
            project.ProjectStatusID,
            project.PriorityID,
            project.IssueTypeID,
            project.ResponsibleID,
            project.SupportID,
            project.WorkpackageID);
    }
}
