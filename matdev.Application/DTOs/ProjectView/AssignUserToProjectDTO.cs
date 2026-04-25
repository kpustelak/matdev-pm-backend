using System.ComponentModel.DataAnnotations;

namespace matdev.Application.DTOs.ProjectView;

public sealed record AssignUserToProjectDTO(
    [param: Range(1, int.MaxValue, ErrorMessage = "User id must be a positive number.")]
    int UserId,
    bool IsResponsible);
