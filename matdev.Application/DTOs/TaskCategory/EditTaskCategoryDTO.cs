using System.ComponentModel.DataAnnotations;

namespace matdev.Application.DTOs.TaskCategory;

public sealed record EditTaskCategoryDTO(
    [param: Required(ErrorMessage = "Task category id is required.")]
    [param: Range(1, int.MaxValue, ErrorMessage = "Task category id must be a positive number.")]
    int TaskCategoryId,
    [param: Required(ErrorMessage = "Name is required.")]
    [param: StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
    string Name
);
