using matdev.Application.DTOs.Project;
using matdev.Application.DTOs.ProjectView;
using matdev.Application.Interfaces;
using matdev.Domain.Entities;
using matdev.Domain.Interfaces;

namespace matdev.Application.Services;

public class ProjectViewService : IProjectViewService
{
    private readonly IProjectViewRepository _repository;

    public ProjectViewService(IProjectViewRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProjectCreateLookupsDTO> GetProjectCreateFormLookupsAsync()
    {
        var issueTypes = await _repository.GetIssueTypesAsync();
        var topics = await _repository.GetTopicsAsync();
        var workpackages = await _repository.GetWorkpackagesAsync();
        var statuses = await _repository.GetStatusesAsync();
        var priorities = await _repository.GetPrioritiesAsync();
        var users = await _repository.GetUsersAsync();

        return new ProjectCreateLookupsDTO(
            issueTypes.Select(i => new LookupOption(i.IssueTypeID, i.Name ?? string.Empty)).ToList(),
            topics.Select(t => new LookupOption(t.TopicID, t.Name ?? string.Empty)).ToList(),
            workpackages.Select(w => new LookupOption(w.WorkpackageID, w.Name ?? string.Empty)).ToList(),
            statuses.Select(s => new LookupOption(s.StatusID, s.Name ?? string.Empty)).ToList(),
            priorities.Select(p => new LookupOption(p.PriorityID, p.Name ?? string.Empty)).ToList(),
            users
                .Select(u => new UserLookupOption(
                    u.UserID,
                    u.FirstName ?? string.Empty,
                    u.LastName ?? string.Empty,
                    $"{u.FirstName} {u.LastName}".Trim()))
                .ToList());
    }

    public async Task<GetProjectViewDataDTO> GetProjectPageAsync(int projectId)
    {
        var project = await GetProjectOrThrow(projectId);

        var tasks = await _repository.GetProjectLiveTopLevelTasksAsync(projectId);
        var assignedUsers = await _repository.GetProjectAssignmentsAsync(projectId);

        var taskDtos = tasks.Select(t => new GetProjectViewTaskDTO(
            t.TaskID,
            t.Name,
            t.Status?.Name ?? string.Empty,
            t.Priority?.Name ?? string.Empty,
            t.IsMilestone,
            t.StartDate,
            t.EndDate,
            t.SortOrder));

        var assignedUserDtos = assignedUsers.Select(a => new GetProjectViewAssignedUserDTO(
            a.UserID,
            a.User.FirstName,
            a.User.LastName,
            a.IsResponsible,
            a.User.Email,
            a.User.PhoneNumber));

        var topbarDto = new GetProjectViewTopbarDTO(
            project.IssueTypeID,
            project.IssueType?.Name ?? string.Empty,
            project.TopicID,
            project.Topic?.Name ?? string.Empty,
            project.WorkpackageID,
            project.Workpackage?.Name ?? string.Empty,
            project.ProjectStatusID,
            project.ProjectStatus?.Name ?? string.Empty);

        return new GetProjectViewDataDTO(taskDtos, assignedUserDtos, topbarDto);
    }

    public async Task<IEnumerable<GetProjectViewAssignedUserDTO>> GetAssignableUsersAsync(int projectId)
    {
        await GetProjectOrThrow(projectId);
        var users = await _repository.GetUsersNotAssignedToProjectAsync(projectId);

        return users.Select(u => new GetProjectViewAssignedUserDTO(
            u.UserID,
            u.FirstName,
            u.LastName,
            false,
            u.Email,
            u.PhoneNumber));
    }

    public async Task<IEnumerable<GetProjectViewEditProjectFormDTO>> GetEditProjectFormDataAsync(int projectId)
    {
        await GetProjectOrThrow(projectId);
        var issueTypes = await _repository.GetIssueTypesAsync();
        var topics = await _repository.GetTopicsAsync();
        var workpackages = await _repository.GetWorkpackagesAsync();
        var statuses = await _repository.GetStatusesAsync();
        var priorities = await _repository.GetPrioritiesAsync();
        var users = await _repository.GetUsersAsync();

        var result = new List<GetProjectViewEditProjectFormDTO>();
        result.AddRange(issueTypes.Select(i => new GetProjectViewEditProjectFormDTO(i.IssueTypeID, i.Name, null, string.Empty, null, string.Empty, null, string.Empty, null, string.Empty, null, string.Empty, string.Empty)));
        result.AddRange(topics.Select(t => new GetProjectViewEditProjectFormDTO(null, string.Empty, t.TopicID, t.Name, null, string.Empty, null, string.Empty, null, string.Empty, null, string.Empty, string.Empty)));
        result.AddRange(workpackages.Select(w => new GetProjectViewEditProjectFormDTO(null, string.Empty, null, string.Empty, w.WorkpackageID, w.Name, null, string.Empty, null, string.Empty, null, string.Empty, string.Empty)));
        result.AddRange(statuses.Select(s => new GetProjectViewEditProjectFormDTO(null, string.Empty, null, string.Empty, null, string.Empty, s.StatusID, s.Name, null, string.Empty, null, string.Empty, string.Empty)));
        result.AddRange(priorities.Select(p => new GetProjectViewEditProjectFormDTO(null, string.Empty, null, string.Empty, null, string.Empty, null, string.Empty, p.PriorityID, p.Name, null, string.Empty, string.Empty)));
        result.AddRange(users.Select(u => new GetProjectViewEditProjectFormDTO(null, string.Empty, null, string.Empty, null, string.Empty, null, string.Empty, null, string.Empty, u.UserID, u.FirstName, u.LastName)));

        return result;
    }

    public async Task ChangeProjectStatusAsync(int projectId, ChangeProjectStatusDTO dto)
    {
        var project = await GetProjectOrThrow(projectId);
        project.ProjectStatusID = dto.ProjectStatusId;
        await _repository.UpdateProjectAsync(project);
    }

    public async Task ChangeProjectDeadlineAsync(int projectId, ChangeProjectDeadlineDTO dto)
    {
        var project = await GetProjectOrThrow(projectId);
        project.EndDate = dto.EndDate;
        await _repository.UpdateProjectAsync(project);
    }

    public async Task AssignUserAsync(int projectId, AssignUserToProjectDTO dto)
    {
        await GetProjectOrThrow(projectId);

        var user = await _repository.GetUserByIdAsync(dto.UserId);
        if (user is null)
            throw new KeyNotFoundException($"User with id {dto.UserId} was not found.");

        var existing = await _repository.GetProjectAssignmentAsync(projectId, dto.UserId);
        if (existing is not null)
            throw new ArgumentException("The user is already assigned to this project.");

        var assignment = new ProjectAssignment
        {
            ProjectID = projectId,
            UserID = dto.UserId,
            IsResponsible = dto.IsResponsible
        };

        await _repository.AddProjectAssignmentAsync(assignment);
    }

    public async Task RemoveUserAsync(int projectId, int userId)
    {
        await GetProjectOrThrow(projectId);

        var existing = await _repository.GetProjectAssignmentAsync(projectId, userId);
        if (existing is null)
            throw new KeyNotFoundException($"Project assignment for project {projectId} and user {userId} was not found.");

        await _repository.DeleteProjectAssignmentAsync(existing);
    }

    private async Task<Project> GetProjectOrThrow(int projectId)
    {
        var project = await _repository.GetProjectDetailsAsync(projectId);
        if (project is null)
            throw new KeyNotFoundException($"Project with id {projectId} was not found.");

        return project;
    }
}
