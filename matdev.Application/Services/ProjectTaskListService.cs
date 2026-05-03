using matdev.Application.DTOs.ProjectTaskList;
using matdev.Application.Interfaces;
using matdev.Domain.Entities;
using matdev.Domain.Entities.TaskEntities;
using matdev.Domain.Interfaces;

namespace matdev.Application.Services;

public class ProjectTaskListService : IProjectTaskListService
{
    private readonly IProjectViewRepository _repository;

    public ProjectTaskListService(IProjectViewRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetProjectTaskListPageDTO> GetTaskListPageAsync(int projectId, TaskListQueryParameters query)
    {
        await GetProjectOrThrow(projectId);

        var effectivePage = Math.Max(1, query.Page);
        var effectivePageSize = Math.Clamp(query.PageSize, 1, 100);
        var sortBy = ParseTaskListSortBy(query.SortBy);
        var (items, total) = await _repository.GetProjectTopLevelTasksListPageAsync(
            projectId,
            effectivePage,
            effectivePageSize,
            query.Search,
            query.MilestonesOnly,
            sortBy,
            query.SortDescending);

        var dtos = items.Select(MapToTaskListItem).ToList();
        return new GetProjectTaskListPageDTO(dtos, total, effectivePage, effectivePageSize);
    }

    public async Task<IReadOnlyList<GetProjectTaskListItemDTO>> GetSubtasksAsync(int projectId, int parentTaskId)
    {
        await GetProjectOrThrow(projectId);

        var parent = await _repository.GetProjectTaskByIdAsync(projectId, parentTaskId);
        if (parent is null)
            throw new KeyNotFoundException($"Task with id {parentTaskId} was not found in project {projectId}.");

        var subtasks = await _repository.GetProjectTaskSubtasksAsync(projectId, parentTaskId);
        return subtasks.Select(MapToTaskListItem).ToList();
    }

    public async Task<GetTaskCreateFormDTO> GetCreateFormAsync(int projectId)
    {
        await GetProjectOrThrow(projectId);

        var users = await _repository.GetUsersAsync();
        var statuses = await _repository.GetStatusesAsync();
        var priorities = await _repository.GetPrioritiesAsync();
        var categories = await _repository.GetTaskCategoriesAsync();

        return new GetTaskCreateFormDTO(
            users.Select(u => new TaskCreateFormUserOptionDTO(u.UserID, u.FirstName, u.LastName)).OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList(),
            statuses.Select(s => new TaskCreateFormLookupDTO(s.StatusID, s.Name)).OrderBy(s => s.Name).ToList(),
            priorities.Select(p => new TaskCreateFormLookupDTO(p.PriorityID, p.Name)).OrderBy(p => p.Name).ToList(),
            categories.Select(c => new TaskCreateFormLookupDTO(c.TaskCategoryID, c.Name)).ToList());
    }

    public async Task<GetProjectTaskListItemDTO> CreateTaskAsync(int projectId, CreateProjectTaskDTO dto)
    {
        await GetProjectOrThrow(projectId);

        var status = await _repository.GetStatusByIdAsync(dto.StatusId);
        if (status is null)
            throw new KeyNotFoundException($"Status with id {dto.StatusId} was not found.");

        var priority = await _repository.GetPriorityByIdAsync(dto.PriorityId);
        if (priority is null)
            throw new KeyNotFoundException($"Priority with id {dto.PriorityId} was not found.");

        if (dto.RequesterId is int requesterId)
        {
            var requester = await _repository.GetUserByIdAsync(requesterId);
            if (requester is null)
                throw new KeyNotFoundException($"Requester user with id {requesterId} was not found.");
        }

        if (dto.TaskCategoryId is int categoryId)
        {
            var category = await _repository.GetTaskCategoryByIdAsync(categoryId);
            if (category is null)
                throw new KeyNotFoundException($"Task category with id {categoryId} was not found.");
        }

        var assignedIds = dto.AssignedUserIds ?? Array.Empty<int>();
        foreach (var userId in assignedIds.Distinct())
        {
            var user = await _repository.GetUserByIdAsync(userId);
            if (user is null)
                throw new KeyNotFoundException($"User with id {userId} was not found.");
        }

        int? parentId = null;
        int sortOrder;
        if (dto.ParentTaskId is int parentTaskId)
        {
            var parent = await _repository.GetProjectTaskByIdAsync(projectId, parentTaskId);
            if (parent is null)
                throw new KeyNotFoundException($"Parent task with id {parentTaskId} was not found in project {projectId}.");

            parentId = parentTaskId;
            sortOrder = await _repository.GetNextSubtaskSortOrderAsync(projectId, parentTaskId);
        }
        else
        {
            sortOrder = await _repository.GetNextTopLevelTaskSortOrderAsync(projectId);
        }

        var endDate = dto.EndDate ?? dto.StartDate;

        var task = new _Task
        {
            ProjectID = projectId,
            Name = dto.Name.Trim(),
            Description = dto.TaskDescription.Trim(),
            StatusID = dto.StatusId,
            PriorityID = dto.PriorityId,
            IsMilestone = dto.IsMilestone,
            StartDate = dto.StartDate,
            EndDate = endDate,
            Progress = 0,
            SortOrder = sortOrder,
            ParentID = parentId,
            RequesterID = dto.RequesterId,
            TaskCategoryID = dto.TaskCategoryId
        };

        await _repository.AddTaskWithAssignmentsAsync(task, assignedIds.Distinct().ToList());

        var created = await _repository.GetProjectTaskByIdAsync(projectId, task.TaskID);
        if (created is null)
            throw new InvalidOperationException("Task was created but could not be reloaded.");

        return MapToTaskListItem(created);
    }

    public async Task ChangeTaskStatusAsync(int projectId, int taskId, ChangeTaskStatusDTO dto)
    {
        await GetProjectOrThrow(projectId);
        var task = await _repository.GetProjectTaskByIdAsync(projectId, taskId);
        if (task is null)
            throw new KeyNotFoundException($"Task with id {taskId} was not found in project {projectId}.");

        var status = await _repository.GetStatusByIdAsync(dto.TaskStatusId);
        if (status is null)
            throw new KeyNotFoundException($"Status with id {dto.TaskStatusId} was not found.");

        task.StatusID = dto.TaskStatusId;
        await _repository.UpdateTaskAsync(task);
    }

    public async Task ChangeTaskPriorityAsync(int projectId, int taskId, ChangeTaskPriorityDTO dto)
    {
        await GetProjectOrThrow(projectId);
        var task = await _repository.GetProjectTaskByIdAsync(projectId, taskId);
        if (task is null)
            throw new KeyNotFoundException($"Task with id {taskId} was not found in project {projectId}.");

        var priority = await _repository.GetPriorityByIdAsync(dto.TaskPriorityId);
        if (priority is null)
            throw new KeyNotFoundException($"Priority with id {dto.TaskPriorityId} was not found.");

        task.PriorityID = dto.TaskPriorityId;
        await _repository.UpdateTaskAsync(task);
    }

    public async Task ChangeTaskEndDateAsync(int projectId, int taskId, ChangeTaskEndDateDTO dto)
    {
        await GetProjectOrThrow(projectId);
        var task = await _repository.GetProjectTaskByIdAsync(projectId, taskId);
        if (task is null)
            throw new KeyNotFoundException($"Task with id {taskId} was not found in project {projectId}.");

        task.EndDate = dto.EndDate;
        await _repository.UpdateTaskAsync(task);
    }

    public async Task UpdateTaskSortOrderAsync(int projectId, int taskId, UpdateTaskSortOrderDTO dto)
    {
        await GetProjectOrThrow(projectId);
        var task = await _repository.GetProjectTaskByIdAsync(projectId, taskId);
        if (task is null)
            throw new KeyNotFoundException($"Task with id {taskId} was not found in project {projectId}.");

        task.SortOrder = dto.NewPosition;
        await _repository.UpdateTaskAsync(task);
    }

    public async Task DeleteTaskAsync(int projectId, int taskId)
    {
        await GetProjectOrThrow(projectId);
        var task = await _repository.GetProjectTaskByIdAsync(projectId, taskId);
        if (task is null)
            throw new KeyNotFoundException($"Task with id {taskId} was not found in project {projectId}.");

        await _repository.DeleteTaskAsync(task);
    }

    private async Task<Project> GetProjectOrThrow(int projectId)
    {
        var project = await _repository.GetProjectDetailsAsync(projectId);
        if (project is null)
            throw new KeyNotFoundException($"Project with id {projectId} was not found.");

        return project;
    }

    private static GetProjectTaskListItemDTO MapToTaskListItem(_Task t)
    {
        return new GetProjectTaskListItemDTO(
            t.TaskID,
            t.Name,
            t.Status?.Name ?? string.Empty,
            t.Priority?.Name ?? string.Empty,
            t.StatusID,
            t.PriorityID,
            t.IsMilestone,
            t.StartDate,
            t.EndDate,
            t.ParentID,
            t.SortOrder,
            t.Progress,
            t.TaskCategoryID,
            t.TaskCategory?.Name);
    }

    private static TaskListSortBy ParseTaskListSortBy(string? sortBy)
    {
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "startdate" => TaskListSortBy.StartDate,
            "enddate" => TaskListSortBy.EndDate,
            "name" => TaskListSortBy.Name,
            _ => TaskListSortBy.SortOrder
        };
    }
}
