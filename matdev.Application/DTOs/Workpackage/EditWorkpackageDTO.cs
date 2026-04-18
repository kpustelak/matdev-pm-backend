using System.ComponentModel.DataAnnotations;

namespace matdev.Application.DTOs.Workpackage;

public sealed record EditWorkpackageDTO(
    [param: Required(ErrorMessage = "Workpackage id is required.")]
    [param: Range(1, int.MaxValue, ErrorMessage = "Workpackage id must be a positive number.")]
    int WorkpackageId,
    [param: Required(ErrorMessage = "Name is required.")]
    [param: StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
    string Name
);
