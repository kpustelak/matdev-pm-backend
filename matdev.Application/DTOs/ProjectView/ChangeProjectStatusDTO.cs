using System.ComponentModel.DataAnnotations;

namespace matdev.Application.DTOs.ProjectView;

public sealed record ChangeProjectStatusDTO(
    [param: Range(1, int.MaxValue, ErrorMessage = "Project status id must be a positive number.")]
    int ProjectStatusId);
