namespace matdev.Application.DTOs.ProjectTaskList;

public sealed record GetProjectTaskListPageDTO(
    IReadOnlyList<GetProjectTaskListItemDTO> Items,
    int TotalCount,
    int Page,
    int PageSize);
