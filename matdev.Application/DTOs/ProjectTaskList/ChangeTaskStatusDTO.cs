using System.ComponentModel.DataAnnotations;

namespace matdev.Application.DTOs.ProjectTaskList;

public sealed record ChangeTaskStatusDTO(
    [param: Range(1, int.MaxValue, ErrorMessage = "Task status id must be a positive number.")]
    int TaskStatusId);
