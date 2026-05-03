namespace matdev.Application.DTOs.ProjectTaskList;

public sealed record TaskCreateFormUserOptionDTO(int UserId, string FirstName, string LastName);

public sealed record TaskCreateFormLookupDTO(int Id, string Name);

public sealed record GetTaskCreateFormDTO(
    IReadOnlyList<TaskCreateFormUserOptionDTO> Users,
    IReadOnlyList<TaskCreateFormLookupDTO> Statuses,
    IReadOnlyList<TaskCreateFormLookupDTO> Priorities,
    IReadOnlyList<TaskCreateFormLookupDTO> TaskCategories);
