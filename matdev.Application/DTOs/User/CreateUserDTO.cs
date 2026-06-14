using System.ComponentModel.DataAnnotations;

namespace matdev.Application.DTOs.User;

public sealed record CreateUserDTO(
    [param: Required(ErrorMessage = "First name is required.")]
    [param: StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters.")]
    string FirstName,

    [param: Required(ErrorMessage = "Last name is required.")]
    [param: StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters.")]
    string LastName,

    [param: EmailAddress(ErrorMessage = "Email format is not valid.")]
    [param: StringLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
    string? Email,

    [param: StringLength(50, ErrorMessage = "Phone number cannot exceed 50 characters.")]
    string? PhoneNumber);
