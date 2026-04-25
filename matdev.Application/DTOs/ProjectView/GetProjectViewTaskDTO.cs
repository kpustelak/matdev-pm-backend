namespace matdev.Application.DTOs.ProjectView;

public sealed record GetProjectViewTaskDTO(
    int TaskId,
    string Name,
    string Status,
    string Priority,
    bool IsMilestone,
    DateTime StartDate,
    DateTime EndDate,
    int SortOrder);
