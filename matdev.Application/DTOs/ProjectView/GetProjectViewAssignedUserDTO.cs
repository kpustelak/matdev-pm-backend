namespace matdev.Application.DTOs.ProjectView;

public sealed record GetProjectViewAssignedUserDTO(
    int UserId,
    string FirstName,
    string LastName,
    bool IsResponsible,
    string Email,
    string PhoneNumber);
