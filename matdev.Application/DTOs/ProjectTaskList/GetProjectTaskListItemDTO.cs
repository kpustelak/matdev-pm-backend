namespace matdev.Application.DTOs.ProjectTaskList;

public sealed record GetProjectTaskListItemDTO(
    int TaskId,
    string Name,
    string Status,
    string Priority,
    int? StatusId,
    int? PriorityId,
    bool IsMilestone,
    DateTime StartDate,
    DateTime EndDate,
    int? ParentId,
    int SortOrder,
    decimal Progress,
    int? TaskCategoryId,
    string? TaskCategoryName);
