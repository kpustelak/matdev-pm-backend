namespace matdev.Application.DTOs.TaskView;

public record GetTaskViewTopbarDTO(
    int TaskId,
    string TaskName,
    string? TaskCategory,
    int? TaskCategoryId,
    string TaskDescription,
    string StatusName,
    int? StatusId,
    DateTime? TaskDeadline,
    string? RequesterFirstName,
    string? RequesterLastName,
    string TaskPriority,
    int? PriorityId,
    decimal TaskProgress,
    bool IsMilestone
);
