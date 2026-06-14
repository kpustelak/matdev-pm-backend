namespace matdev.Application.DTOs.TaskCategory;

public sealed record GetTaskCategoryDTO(
    int TaskCategoryId,
    string Name
);
