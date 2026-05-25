using matdev.Application.DTOs.ProjectTaskList;
using matdev.Application.DTOs.TaskView;
using matdev.Application.Interfaces;
using matdev.Domain.Entities;
using matdev.Domain.Entities.TaskEntities;
using matdev.Domain.Interfaces;

namespace matdev.Application.Services;

/// <summary>Implements the Task View (Layer 3) page — task detail, subtasks, assignments, and mutations.</summary>
public class TaskViewService : ITaskViewService
{
    private readonly IProjectViewRepository _repository;

    public TaskViewService(IProjectViewRepository repository)
    {
        _repository = repository;
    }

    // ── Main view ────────────────────────────────────────────────────────────

    public async Task<GetTaskViewDTO> GetTaskViewAsync(int projectId, int taskId)
    {
        await GetProjectOrThrow(projectId);
        var task = await GetTaskOrThrow(projectId, taskId, fullLoad: true);

        var topbar = MapToTopbar(task);

        var subtasks = await _repository.GetProjectTaskSubtasksAsync(projectId, taskId);
        var subtaskDtos = subtasks.Select(MapToSubtask).ToList();

        var assignments = await _repository.GetTaskAssignmentsAsync(taskId);
        var assignmentDtos = assignments.Select(a => new GetTaskViewAssignedUserDTO(
            a.User.UserID, a.User.FirstName, a.User.LastName)).ToList();

        return new GetTaskViewDTO(topbar, subtaskDtos, assignmentDtos);
    }

    // ── Topbar mutations ─────────────────────────────────────────────────────

    public async Task ChangeTaskStatusAsync(int projectId, int taskId, ChangeTaskViewStatusDTO dto)
    {
        await GetProjectOrThrow(projectId);
        var task = await GetTaskOrThrow(projectId, taskId);

        var status = await _repository.GetStatusByIdAsync(dto.TaskStatusId);
        if (status is null)
            throw new KeyNotFoundException($"Status with id {dto.TaskStatusId} was not found.");

        task.StatusID = dto.TaskStatusId;
        await _repository.UpdateTaskAsync(task);
    }

    public async Task ChangeTaskDeadlineAsync(int projectId, int taskId, ChangeTaskViewDeadlineDTO dto)
    {
        await GetProjectOrThrow(projectId);
        var task = await GetTaskOrThrow(projectId, taskId);

        task.EndDate = dto.EndDate;
        await _repository.UpdateTaskAsync(task);
    }

    public async Task ChangeTaskPriorityAsync(int projectId, int taskId, ChangeTaskViewPriorityDTO dto)
    {
        await GetProjectOrThrow(projectId);
        var task = await GetTaskOrThrow(projectId, taskId);

        var priority = await _repository.GetPriorityByIdAsync(dto.TaskPriorityId);
        if (priority is null)
            throw new KeyNotFoundException($"Priority with id {dto.TaskPriorityId} was not found.");

        task.PriorityID = dto.TaskPriorityId;
        await _repository.UpdateTaskAsync(task);
    }

    // ── Subtask form + CRUD ──────────────────────────────────────────────────

    public async Task<GetCreateSubtaskFormDTO> GetCreateSubtaskFormAsync(int projectId)
    {
        await GetProjectOrThrow(projectId);

        var categories = await _repository.GetTaskCategoriesAsync();
        var statuses = await _repository.GetStatusesAsync();
        var priorities = await _repository.GetPrioritiesAsync();
        var users = await _repository.GetUsersAsync();

        return new GetCreateSubtaskFormDTO(
            categories.Select(c => new TaskCreateFormLookupDTO(c.TaskCategoryID, c.Name)).ToList(),
            statuses.Select(s => new TaskCreateFormLookupDTO(s.StatusID, s.Name)).OrderBy(s => s.Name).ToList(),
            priorities.Select(p => new TaskCreateFormLookupDTO(p.PriorityID, p.Name)).OrderBy(p => p.Name).ToList(),
            users.Select(u => new TaskCreateFormUserOptionDTO(u.UserID, u.FirstName, u.LastName))
                .OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList());
    }

    public async Task<GetTaskViewSubtaskDTO> CreateSubtaskAsync(int projectId, int parentTaskId, CreateSubtaskDTO dto)
    {
        await GetProjectOrThrow(projectId);

        var parent = await _repository.GetProjectTaskByIdAsync(projectId, parentTaskId);
        if (parent is null)
            throw new KeyNotFoundException($"Task with id {parentTaskId} was not found in project {projectId}.");

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
        foreach (var uid in assignedIds.Distinct())
        {
            var user = await _repository.GetUserByIdAsync(uid);
            if (user is null)
                throw new KeyNotFoundException($"User with id {uid} was not found.");
        }

        var sortOrder = await _repository.GetNextSubtaskSortOrderAsync(projectId, parentTaskId);
        var endDate = dto.EndDate ?? dto.StartDate;

        var subtask = new _Task
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
            ParentID = parentTaskId,
            RequesterID = dto.RequesterId,
            TaskCategoryID = dto.TaskCategoryId
        };

        await _repository.AddTaskWithAssignmentsAsync(subtask, assignedIds.Distinct().ToList());

        var created = await _repository.GetProjectTaskByIdAsync(projectId, subtask.TaskID);
        if (created is null)
            throw new InvalidOperationException("Subtask was created but could not be reloaded.");

        return MapToSubtask(created);
    }

    public async Task DeleteSubtaskAsync(int projectId, int subtaskId)
    {
        await GetProjectOrThrow(projectId);
        var subtask = await GetTaskOrThrow(projectId, subtaskId);

        if (subtask.ParentID is null)
            throw new ArgumentException($"Task {subtaskId} is not a subtask and cannot be deleted through this endpoint.");

        await _repository.DeleteTaskAsync(subtask);
    }

    public async Task ChangeSubtaskStatusAsync(int projectId, int subtaskId, ChangeSubtaskStatusDTO dto)
    {
        await GetProjectOrThrow(projectId);
        var subtask = await GetTaskOrThrow(projectId, subtaskId);

        var status = await _repository.GetStatusByIdAsync(dto.StatusId);
        if (status is null)
            throw new KeyNotFoundException($"Status with id {dto.StatusId} was not found.");

        subtask.StatusID = dto.StatusId;
        await _repository.UpdateTaskAsync(subtask);

        await RecalculateParentProgressAsync(projectId, subtask.ParentID);
    }

    public async Task ChangeSubtaskStartDateAsync(int projectId, int subtaskId, ChangeSubtaskStartDateDTO dto)
    {
        await GetProjectOrThrow(projectId);
        var subtask = await GetTaskOrThrow(projectId, subtaskId);

        subtask.StartDate = dto.StartDate;
        await _repository.UpdateTaskAsync(subtask);
    }

    // ── Edit task ────────────────────────────────────────────────────────────

    public async Task<GetTaskEditFormDTO> GetEditFormAsync(int projectId)
    {
        await GetProjectOrThrow(projectId);

        var statuses = await _repository.GetStatusesAsync();
        var priorities = await _repository.GetPrioritiesAsync();
        var categories = await _repository.GetTaskCategoriesAsync();
        var users = await _repository.GetUsersAsync();

        return new GetTaskEditFormDTO(
            statuses.Select(s => new TaskCreateFormLookupDTO(s.StatusID, s.Name)).OrderBy(s => s.Name).ToList(),
            priorities.Select(p => new TaskCreateFormLookupDTO(p.PriorityID, p.Name)).OrderBy(p => p.Name).ToList(),
            categories.Select(c => new TaskCreateFormLookupDTO(c.TaskCategoryID, c.Name)).ToList(),
            users.Select(u => new TaskCreateFormUserOptionDTO(u.UserID, u.FirstName, u.LastName))
                .OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList());
    }

    public async Task<GetTaskViewDTO> EditTaskAsync(int projectId, int taskId, EditTaskDTO dto)
    {
        await GetProjectOrThrow(projectId);
        var task = await GetTaskOrThrow(projectId, taskId);

        if (dto.Name is not null)
            task.Name = dto.Name.Trim();

        if (dto.StatusId is int statusId)
        {
            var status = await _repository.GetStatusByIdAsync(statusId);
            if (status is null)
                throw new KeyNotFoundException($"Status with id {statusId} was not found.");
            task.StatusID = statusId;
        }

        if (dto.PriorityId is int priorityId)
        {
            var priority = await _repository.GetPriorityByIdAsync(priorityId);
            if (priority is null)
                throw new KeyNotFoundException($"Priority with id {priorityId} was not found.");
            task.PriorityID = priorityId;
        }

        if (dto.IsMilestone is bool isMilestone)
            task.IsMilestone = isMilestone;

        if (dto.RequesterId is int requesterId)
        {
            var requester = await _repository.GetUserByIdAsync(requesterId);
            if (requester is null)
                throw new KeyNotFoundException($"Requester user with id {requesterId} was not found.");
            task.RequesterID = requesterId;
        }

        if (dto.StartDate is DateTime startDate)
            task.StartDate = startDate;

        if (dto.EndDate is DateTime endDate)
            task.EndDate = endDate;

        if (dto.TaskDescription is not null)
            task.Description = dto.TaskDescription.Trim();

        if (dto.TaskCategoryId is int categoryId)
        {
            var category = await _repository.GetTaskCategoryByIdAsync(categoryId);
            if (category is null)
                throw new KeyNotFoundException($"Task category with id {categoryId} was not found.");
            task.TaskCategoryID = categoryId;
        }

        if (dto.AssignedUserIds is not null)
        {
            foreach (var uid in dto.AssignedUserIds.Distinct())
            {
                var user = await _repository.GetUserByIdAsync(uid);
                if (user is null)
                    throw new KeyNotFoundException($"User with id {uid} was not found.");
            }
            await _repository.UpdateTaskWithAssignmentsAsync(task, dto.AssignedUserIds.Distinct().ToList());
        }
        else
        {
            await _repository.UpdateTaskAsync(task);
        }

        return await GetTaskViewAsync(projectId, taskId);
    }

    // ── Task assignments ─────────────────────────────────────────────────────

    public async Task<IReadOnlyList<GetTaskViewAssignedUserDTO>> GetTaskAssignmentsAsync(int projectId, int taskId)
    {
        await GetProjectOrThrow(projectId);
        await GetTaskOrThrow(projectId, taskId);

        var assignments = await _repository.GetTaskAssignmentsAsync(taskId);
        return assignments.Select(a => new GetTaskViewAssignedUserDTO(
            a.User.UserID, a.User.FirstName, a.User.LastName)).ToList();
    }

    public async Task AssignUserToTaskAsync(int projectId, int taskId, AssignUserToTaskDTO dto)
    {
        await GetProjectOrThrow(projectId);
        await GetTaskOrThrow(projectId, taskId);

        var user = await _repository.GetUserByIdAsync(dto.UserId);
        if (user is null)
            throw new KeyNotFoundException($"User with id {dto.UserId} was not found.");

        var existing = await _repository.GetTaskAssignmentAsync(taskId, dto.UserId);
        if (existing is not null)
            throw new InvalidOperationException($"User {dto.UserId} is already assigned to task {taskId}.");

        await _repository.AddTaskAssignmentAsync(new TaskAssignment
        {
            TaskID = taskId,
            UserID = dto.UserId
        });
    }

    public async Task RemoveUserFromTaskAsync(int projectId, int taskId, int userId)
    {
        await GetProjectOrThrow(projectId);
        await GetTaskOrThrow(projectId, taskId);

        var assignment = await _repository.GetTaskAssignmentAsync(taskId, userId);
        if (assignment is null)
            throw new KeyNotFoundException($"User {userId} is not assigned to task {taskId}.");

        await _repository.DeleteTaskAssignmentAsync(assignment);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<Project> GetProjectOrThrow(int projectId)
    {
        var project = await _repository.GetProjectDetailsAsync(projectId);
        if (project is null)
            throw new KeyNotFoundException($"Project with id {projectId} was not found.");
        return project;
    }

    private async Task<_Task> GetTaskOrThrow(int projectId, int taskId, bool fullLoad = false)
    {
        var task = fullLoad
            ? await _repository.GetTaskViewDataAsync(projectId, taskId)
            : await _repository.GetProjectTaskByIdAsync(projectId, taskId);

        if (task is null)
            throw new KeyNotFoundException($"Task with id {taskId} was not found in project {projectId}.");

        return task;
    }

    private async Task RecalculateParentProgressAsync(int projectId, int? parentId)
    {
        if (parentId is null) return;

        var parent = await _repository.GetProjectTaskByIdAsync(projectId, parentId.Value);
        if (parent is null) return;

        var siblings = await _repository.GetProjectTaskSubtasksAsync(projectId, parentId.Value);
        if (!siblings.Any()) return;

        var completedCount = siblings.Count(s =>
            s.Status?.Name.Equals("DONE", StringComparison.OrdinalIgnoreCase) == true ||
            s.Status?.Name.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase) == true);

        parent.Progress = Math.Round((decimal)completedCount / siblings.Count * 100, 2);
        await _repository.UpdateTaskAsync(parent);
    }

    private static GetTaskViewTopbarDTO MapToTopbar(_Task t) => new(
        t.TaskID,
        t.Name,
        t.TaskCategory?.Name,
        t.TaskCategoryID,
        t.Description,
        t.Status?.Name ?? string.Empty,
        t.StatusID,
        t.EndDate,
        t.Requester?.FirstName,
        t.Requester?.LastName,
        t.Priority?.Name ?? string.Empty,
        t.PriorityID,
        t.Progress,
        t.IsMilestone
    );

    private static GetTaskViewSubtaskDTO MapToSubtask(_Task t) => new(
        t.TaskID,
        t.Name,
        t.Status?.Name ?? string.Empty,
        t.Priority?.Name ?? string.Empty,
        t.StatusID,
        t.PriorityID,
        t.StartDate,
        t.EndDate,
        t.SortOrder,
        t.TaskCategoryID,
        t.TaskCategory?.Name
    );
}
