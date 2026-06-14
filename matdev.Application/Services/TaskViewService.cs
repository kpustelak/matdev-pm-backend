using matdev.Application.DTOs.ProjectTaskList;
using matdev.Application.DTOs.TaskView;
using matdev.Application.Interfaces;
using matdev.Domain.Entities;
using matdev.Domain.Entities.TaskEntities;
using matdev.Domain.Interfaces;

namespace matdev.Application.Services;

public class TaskViewService : ITaskViewService
{
    private readonly IProjectViewRepository _repository;
    private readonly IBudgetRepository _budgetRepository;

    public TaskViewService(IProjectViewRepository repository, IBudgetRepository budgetRepository)
    {
        _repository = repository;
        _budgetRepository = budgetRepository;
    }

    public async Task<GetTaskViewDTO> GetTaskViewAsync(int projectId, int taskId)
    {
        await RequireProjectAsync(projectId);
        var task = await RequireTaskAsync(projectId, taskId, fullLoad: true);

        var subtasks = await _repository.GetProjectTaskSubtasksAsync(projectId, taskId);
        var assignments = await _repository.GetTaskAssignmentsAsync(taskId);

        var costs = await BuildTaskCostsAsync(projectId, taskId, task);

        return new GetTaskViewDTO(
            MapToTopbar(task),
            subtasks.Select(MapToSubtask).ToList(),
            MapAssignments(assignments),
            costs);
    }

    public async Task ChangeTaskStatusAsync(int projectId, int taskId, ChangeTaskViewStatusDTO dto)
    {
        await RequireProjectAsync(projectId);
        var task = await RequireTaskAsync(projectId, taskId);

        await RequireStatusAsync(dto.TaskStatusId);
        task.StatusID = dto.TaskStatusId;
        await _repository.UpdateTaskAsync(task);
    }

    public async Task ChangeTaskDeadlineAsync(int projectId, int taskId, ChangeTaskViewDeadlineDTO dto)
    {
        await RequireProjectAsync(projectId);
        var task = await RequireTaskAsync(projectId, taskId);

        task.EndDate = dto.EndDate;
        await _repository.UpdateTaskAsync(task);
    }

    public async Task ChangeTaskPriorityAsync(int projectId, int taskId, ChangeTaskViewPriorityDTO dto)
    {
        await RequireProjectAsync(projectId);
        var task = await RequireTaskAsync(projectId, taskId);

        await RequirePriorityAsync(dto.TaskPriorityId);
        task.PriorityID = dto.TaskPriorityId;
        await _repository.UpdateTaskAsync(task);
    }

    public async Task<GetCreateSubtaskFormDTO> GetCreateSubtaskFormAsync(int projectId)
    {
        await RequireProjectAsync(projectId);
        var (statuses, priorities, categories, users) = await LoadFormDataAsync();

        return new GetCreateSubtaskFormDTO(categories, statuses, priorities, users);
    }

    public async Task<GetTaskViewSubtaskDTO> CreateSubtaskAsync(int projectId, int parentTaskId, CreateSubtaskDTO dto)
    {
        await RequireProjectAsync(projectId);
        await RequireTaskAsync(projectId, parentTaskId);
        await RequireStatusAsync(dto.StatusId);
        await RequirePriorityAsync(dto.PriorityId);

        if (dto.RequesterId is int requesterId)
            await RequireUserAsync(requesterId);

        if (dto.TaskCategoryId is int categoryId)
            await RequireCategoryAsync(categoryId);

        var assignedIds = (dto.AssignedUserIds ?? Array.Empty<int>()).Distinct().ToList();
        foreach (var uid in assignedIds)
            await RequireUserAsync(uid);

        var subtask = new _Task
        {
            ProjectID = projectId,
            Name = dto.Name.Trim(),
            Description = dto.TaskDescription.Trim(),
            StatusID = dto.StatusId,
            PriorityID = dto.PriorityId,
            IsMilestone = dto.IsMilestone,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate ?? dto.StartDate,
            Progress = 0,
            SortOrder = await _repository.GetNextSubtaskSortOrderAsync(projectId, parentTaskId),
            ParentID = parentTaskId,
            RequesterID = dto.RequesterId,
            TaskCategoryID = dto.TaskCategoryId
        };

        await _repository.AddTaskWithAssignmentsAsync(subtask, assignedIds);

        var created = await _repository.GetProjectTaskByIdAsync(projectId, subtask.TaskID)
            ?? throw new InvalidOperationException("Subtask was created but could not be reloaded.");

        return MapToSubtask(created);
    }

    public async Task DeleteSubtaskAsync(int projectId, int subtaskId)
    {
        await RequireProjectAsync(projectId);
        var subtask = await RequireTaskAsync(projectId, subtaskId);

        if (subtask.ParentID is null)
            throw new ArgumentException($"Task {subtaskId} is not a subtask and cannot be deleted through this endpoint.");

        await _repository.DeleteTaskAsync(subtask);
    }

    public async Task ChangeSubtaskStatusAsync(int projectId, int subtaskId, ChangeSubtaskStatusDTO dto)
    {
        await RequireProjectAsync(projectId);
        var subtask = await RequireTaskAsync(projectId, subtaskId);

        await RequireStatusAsync(dto.StatusId);
        subtask.StatusID = dto.StatusId;
        await _repository.UpdateTaskAsync(subtask);

        await RecalculateParentProgressAsync(projectId, subtask.ParentID);
    }

    public async Task ChangeSubtaskStartDateAsync(int projectId, int subtaskId, ChangeSubtaskStartDateDTO dto)
    {
        await RequireProjectAsync(projectId);
        var subtask = await RequireTaskAsync(projectId, subtaskId);

        subtask.StartDate = dto.StartDate;
        await _repository.UpdateTaskAsync(subtask);
    }

    public async Task ChangeSubtaskEndDateAsync(int projectId, int subtaskId, ChangeSubtaskEndDateDTO dto)
    {
        await RequireProjectAsync(projectId);
        var subtask = await RequireTaskAsync(projectId, subtaskId);

        subtask.EndDate = dto.EndDate;
        await _repository.UpdateTaskAsync(subtask);
    }

    public async Task<GetTaskEditFormDTO> GetEditFormAsync(int projectId)
    {
        await RequireProjectAsync(projectId);
        var (statuses, priorities, categories, users) = await LoadFormDataAsync();

        return new GetTaskEditFormDTO(statuses, priorities, categories, users);
    }

    public async Task<GetTaskViewDTO> EditTaskAsync(int projectId, int taskId, EditTaskDTO dto)
    {
        await RequireProjectAsync(projectId);
        var task = await RequireTaskAsync(projectId, taskId);

        if (dto.Name is not null)
            task.Name = dto.Name.Trim();

        if (dto.StatusId is int statusId)
        {
            await RequireStatusAsync(statusId);
            task.StatusID = statusId;
        }

        if (dto.PriorityId is int priorityId)
        {
            await RequirePriorityAsync(priorityId);
            task.PriorityID = priorityId;
        }

        if (dto.IsMilestone is bool isMilestone)   task.IsMilestone = isMilestone;
        if (dto.StartDate is DateTime startDate)    task.StartDate = startDate;
        if (dto.EndDate is DateTime endDate)        task.EndDate = endDate;

        if (dto.TaskDescription is not null)
            task.Description = dto.TaskDescription.Trim();

        if (dto.RequesterId is int requesterId)
        {
            await RequireUserAsync(requesterId);
            task.RequesterID = requesterId;
        }

        if (dto.TaskCategoryId is int categoryId)
        {
            await RequireCategoryAsync(categoryId);
            task.TaskCategoryID = categoryId;
        }

        if (dto.EstimatedCost is decimal estimatedCost)
            task.EstimatedCost = estimatedCost > 0 ? estimatedCost : null;

        if (dto.AssignedUserIds is not null)
        {
            var distinct = dto.AssignedUserIds.Distinct().ToList();
            foreach (var uid in distinct)
                await RequireUserAsync(uid);

            await _repository.UpdateTaskWithAssignmentsAsync(task, distinct);
        }
        else
        {
            await _repository.UpdateTaskAsync(task);
        }

        return await GetTaskViewAsync(projectId, taskId);
    }

    // ── Task assignments ──────────────────────────────────────────────────────

    public async Task<IReadOnlyList<GetTaskViewAssignedUserDTO>> GetTaskAssignmentsAsync(int projectId, int taskId)
    {
        await RequireProjectAsync(projectId);
        await RequireTaskAsync(projectId, taskId);

        return MapAssignments(await _repository.GetTaskAssignmentsAsync(taskId));
    }

    public async Task AssignUserToTaskAsync(int projectId, int taskId, AssignUserToTaskDTO dto)
    {
        await RequireProjectAsync(projectId);
        await RequireTaskAsync(projectId, taskId);
        await RequireUserAsync(dto.UserId);

        var existing = await _repository.GetTaskAssignmentAsync(taskId, dto.UserId);
        if (existing is not null)
            throw new InvalidOperationException($"User {dto.UserId} is already assigned to task {taskId}.");

        await _repository.AddTaskAssignmentAsync(new TaskAssignment { TaskID = taskId, UserID = dto.UserId });
    }

    public async Task RemoveUserFromTaskAsync(int projectId, int taskId, int userId)
    {
        await RequireProjectAsync(projectId);
        await RequireTaskAsync(projectId, taskId);

        var assignment = await _repository.GetTaskAssignmentAsync(taskId, userId)
            ?? throw new KeyNotFoundException($"User {userId} is not assigned to task {taskId}.");

        await _repository.DeleteTaskAssignmentAsync(assignment);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task RequireProjectAsync(int projectId)
    {
        if (await _repository.GetProjectDetailsAsync(projectId) is null)
            throw new KeyNotFoundException($"Project with id {projectId} was not found.");
    }

    private async Task<_Task> RequireTaskAsync(int projectId, int taskId, bool fullLoad = false)
    {
        var task = fullLoad
            ? await _repository.GetTaskViewDataAsync(projectId, taskId)
            : await _repository.GetProjectTaskByIdAsync(projectId, taskId);

        return task ?? throw new KeyNotFoundException($"Task with id {taskId} was not found in project {projectId}.");
    }

    private async Task RequireStatusAsync(int id)
    {
        if (await _repository.GetStatusByIdAsync(id) is null)
            throw new KeyNotFoundException($"Status with id {id} was not found.");
    }

    private async Task RequirePriorityAsync(int id)
    {
        if (await _repository.GetPriorityByIdAsync(id) is null)
            throw new KeyNotFoundException($"Priority with id {id} was not found.");
    }

    private async Task RequireUserAsync(int id)
    {
        if (await _repository.GetUserByIdAsync(id) is null)
            throw new KeyNotFoundException($"User with id {id} was not found.");
    }

    private async Task RequireCategoryAsync(int id)
    {
        if (await _repository.GetTaskCategoryByIdAsync(id) is null)
            throw new KeyNotFoundException($"Task category with id {id} was not found.");
    }

    private async Task<(
        IReadOnlyList<TaskCreateFormLookupDTO> Statuses,
        IReadOnlyList<TaskCreateFormLookupDTO> Priorities,
        IReadOnlyList<TaskCreateFormLookupDTO> Categories,
        IReadOnlyList<TaskCreateFormUserOptionDTO> Users)> LoadFormDataAsync()
    {
        var statuses    = (await _repository.GetStatusesAsync())
                            .Select(s => new TaskCreateFormLookupDTO(s.StatusID, s.Name))
                            .OrderBy(s => s.Name).ToList();
        var priorities  = (await _repository.GetPrioritiesAsync())
                            .Select(p => new TaskCreateFormLookupDTO(p.PriorityID, p.Name))
                            .OrderBy(p => p.Name).ToList();
        var categories  = (await _repository.GetTaskCategoriesAsync())
                            .Select(c => new TaskCreateFormLookupDTO(c.TaskCategoryID, c.Name))
                            .ToList();
        var users       = (await _repository.GetUsersAsync())
                            .Select(u => new TaskCreateFormUserOptionDTO(u.UserID, u.FirstName, u.LastName))
                            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList();

        return (statuses, priorities, categories, users);
    }

    private async Task RecalculateParentProgressAsync(int projectId, int? parentId)
    {
        if (parentId is null) return;

        var parent = await _repository.GetProjectTaskByIdAsync(projectId, parentId.Value);
        if (parent is null) return;

        var siblings = await _repository.GetProjectTaskSubtasksAsync(projectId, parentId.Value);
        if (!siblings.Any()) return;

        var completedCount = siblings.Count(s => IsDoneStatus(s.Status?.Name));

        parent.Progress = Math.Round((decimal)completedCount / siblings.Count * 100, 2);
        await _repository.UpdateTaskAsync(parent);
    }

    private static bool IsDoneStatus(string? status)
    {
        var s = (status ?? string.Empty).Trim().ToLowerInvariant();
        return s.Contains("done") || s.Contains("closed") || s.Contains("completed") || s.Contains("finish");
    }

    private static IReadOnlyList<GetTaskViewAssignedUserDTO> MapAssignments(IEnumerable<TaskAssignment> assignments) =>
        assignments.Select(a => new GetTaskViewAssignedUserDTO(a.User.UserID, a.User.FirstName, a.User.LastName)).ToList();

    private async Task<GetTaskViewCostsDTO> BuildTaskCostsAsync(int projectId, int taskId, _Task task)
    {
        var spent = await _budgetRepository.GetTaskExpenditureSumAsync(projectId, taskId);
        var rows = await _budgetRepository.GetExpendituresForTaskAsync(projectId, taskId);
        var expenditures = rows
            .Select(e => new TaskLinkedExpenditureDTO(
                e.ExpenditureID,
                e.BudgetCategory.Name,
                e.Amount,
                e.TransactionDate,
                e.Description))
            .ToList();

        return new GetTaskViewCostsDTO(task.EstimatedCost, spent, expenditures);
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
        t.IsMilestone,
        t.EstimatedCost);

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
        t.TaskCategory?.Name);
}
