using matdev.Domain.Entities;
using matdev.Domain.Entities.LookupEntities;
using matdev.Domain.Entities.TaskEntities;

namespace matdev.Domain.Interfaces;

public interface IProjectViewRepository
{
    Task<Project?> GetProjectDetailsAsync(int projectId);
    Task<IEnumerable<_Task>> GetProjectLiveTopLevelTasksAsync(int projectId);
    Task<(IReadOnlyList<_Task> Items, int TotalCount)> GetProjectTopLevelTasksListPageAsync(
        int projectId,
        int page,
        int pageSize,
        string? search,
        bool milestonesOnly,
        TaskListSortBy sortBy,
        bool sortDescending);

    Task<IReadOnlyList<_Task>> GetProjectTaskSubtasksAsync(int projectId, int parentTaskId);

    Task<int> GetNextTopLevelTaskSortOrderAsync(int projectId);

    Task<int> GetNextSubtaskSortOrderAsync(int projectId, int parentTaskId);

    Task AddTaskWithAssignmentsAsync(_Task task, IReadOnlyList<int> assignedUserIds);

    Task UpdateTaskAsync(_Task task);

    Task<Status?> GetStatusByIdAsync(int statusId);

    Task<Priority?> GetPriorityByIdAsync(int priorityId);

    Task<TaskCategory?> GetTaskCategoryByIdAsync(int taskCategoryId);

    Task<IReadOnlyList<TaskCategory>> GetTaskCategoriesAsync();

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
