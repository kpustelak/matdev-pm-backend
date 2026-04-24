using matdev.Domain.Entities;
using matdev.Domain.Entities.LookupEntities;
using matdev.Domain.Entities.TaskEntities;

namespace matdev.Domain.Interfaces;

public interface IProjectViewRepository
{
    Task<Project?> GetProjectDetailsAsync(int projectId);
    Task<IEnumerable<_Task>> GetProjectLiveTopLevelTasksAsync(int projectId);
    Task<_Task?> GetProjectTaskByIdAsync(int projectId, int taskId);
    Task<IEnumerable<ProjectAssignment>> GetProjectAssignmentsAsync(int projectId);
    Task<ProjectAssignment?> GetProjectAssignmentAsync(int projectId, int userId);
    Task<IEnumerable<User>> GetUsersNotAssignedToProjectAsync(int projectId);
    Task<IEnumerable<IssueType>> GetIssueTypesAsync();
    Task<IEnumerable<Topic>> GetTopicsAsync();
    Task<IEnumerable<Workpackage>> GetWorkpackagesAsync();
    Task<IEnumerable<Status>> GetStatusesAsync();
    Task<IEnumerable<Priority>> GetPrioritiesAsync();
    Task<IEnumerable<User>> GetUsersAsync();
    Task<User?> GetUserByIdAsync(int userId);
    Task AddProjectAssignmentAsync(ProjectAssignment assignment);
    Task UpdateProjectAsync(Project project);
    Task DeleteProjectAssignmentAsync(ProjectAssignment assignment);
    Task DeleteTaskAsync(_Task task);
}
