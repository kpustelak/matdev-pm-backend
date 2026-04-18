namespace matdev.Application.DTOs.User;

public sealed record GetUserDTO(
    int UserId,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber);
