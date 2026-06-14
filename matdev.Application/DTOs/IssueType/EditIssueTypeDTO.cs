using System.ComponentModel.DataAnnotations;

namespace matdev.Application.DTOs.IssueType;

public sealed record EditIssueTypeDTO(
    [param: Required(ErrorMessage = "Issue type id is required.")]
    [param: Range(1, int.MaxValue, ErrorMessage = "IssueType id must be a positive number.")]
    int IssueTypeId,
    [param: Required(ErrorMessage = "Name is required.")]
    [param: StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
    string Name
);
