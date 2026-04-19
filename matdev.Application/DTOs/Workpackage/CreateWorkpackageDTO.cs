using System.ComponentModel.DataAnnotations;

namespace matdev.Application.DTOs.Workpackage;

public sealed record CreateWorkpackageDTO(
    [param: Required(ErrorMessage = "Name is required.")]
    [param: StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
    string Name
    );
