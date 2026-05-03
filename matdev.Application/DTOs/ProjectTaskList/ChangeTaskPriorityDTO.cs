using System.ComponentModel.DataAnnotations;

namespace matdev.Application.DTOs.ProjectTaskList;

public sealed record ChangeTaskPriorityDTO(
    [param: Range(1, int.MaxValue, ErrorMessage = "Task priority id must be a positive number.")]
    int TaskPriorityId);
