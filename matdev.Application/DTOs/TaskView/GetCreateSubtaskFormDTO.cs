using matdev.Application.DTOs.ProjectTaskList;

namespace matdev.Application.DTOs.TaskView;

public record GetCreateSubtaskFormDTO(
    IReadOnlyList<TaskCreateFormLookupDTO> TaskCategories,
    IReadOnlyList<TaskCreateFormLookupDTO> Statuses,
    IReadOnlyList<TaskCreateFormLookupDTO> Priorities,
    IReadOnlyList<TaskCreateFormUserOptionDTO> Users
);
