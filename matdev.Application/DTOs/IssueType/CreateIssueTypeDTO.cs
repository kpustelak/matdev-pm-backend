using System.ComponentModel.DataAnnotations;

namespace matdev.Application.DTOs.IssueType;

public sealed record CreateIssueTypeDTO(
    [param: Required(ErrorMessage = "Name is required.")]
    [param: StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
    string Name
    );