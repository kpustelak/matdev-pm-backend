using System.ComponentModel.DataAnnotations;

namespace matdev.Application.DTOs.Project;

public sealed record CreateProjectDTO(
    [param: Required(ErrorMessage = "Project name is required.")]
    [param: StringLength(200, MinimumLength = 1, ErrorMessage = "Project name must be between 1 and 200 characters.")]
    string ProjectName,

    [param: Range(1, int.MaxValue, ErrorMessage = "Topic id must be a positive number.")]
    int? TopicId,

    [param: Range(1, int.MaxValue, ErrorMessage = "Status id must be a positive number.")]
    int? StatusId,

    [param: Range(1, int.MaxValue, ErrorMessage = "Priority id must be a positive number.")]
    int? PriorityId,

    [param: Range(1, int.MaxValue, ErrorMessage = "Issue type id must be a positive number.")]
    int? IssuetypeId,

    [param: Range(1, int.MaxValue, ErrorMessage = "Responsible person id must be a positive number.")]
    int? RespPeronId,

    [param: Range(1, int.MaxValue, ErrorMessage = "Supporting person id must be a positive number.")]
    int? SuppPersonId,

    DateOnly? StartDate,
    DateOnly? EndDate,

    [param: Range(1, int.MaxValue, ErrorMessage = "Workpackage id must be a positive number.")]
    int? WorkpackageId,

    [param: StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    string? Description);
