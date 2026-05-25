using matdev.Application.DTOs.ProjectTaskList;

namespace matdev.Application.DTOs.TaskView;

public record GetTaskEditFormDTO(
    IReadOnlyList<TaskCreateFormLookupDTO> Statuses,
    IReadOnlyList<TaskCreateFormLookupDTO> Priorities,
    IReadOnlyList<TaskCreateFormLookupDTO> TaskCategories,
    IReadOnlyList<TaskCreateFormUserOptionDTO> Users
);
