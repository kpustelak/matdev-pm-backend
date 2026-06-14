namespace matdev.Application.DTOs.TaskView;

public record GetTaskViewSubtaskDTO(
    int SubtaskId,
    string SubtaskName,
    string SubtaskStatus,
    string SubtaskPriority,
    int? SubtaskStatusId,
    int? PriorityId,
    DateTime SubtaskStartDate,
    DateTime SubtaskEndDate,
    int SortOrder,
    int? TaskCategoryId,
    string? TaskCategoryName
);
