using System.ComponentModel.DataAnnotations;

namespace matdev.Application.DTOs.ProjectTaskList;

public sealed record UpdateTaskSortOrderDTO(
    [param: Range(0, int.MaxValue)]
    int NewPosition);
