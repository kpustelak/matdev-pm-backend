namespace matdev.Application.DTOs.TaskView;

public record GetTaskViewAssignedUserDTO(
    int UserId,
    string FirstName,
    string LastName
);
