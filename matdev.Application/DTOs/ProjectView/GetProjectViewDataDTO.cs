namespace matdev.Application.DTOs.ProjectView;

public sealed record GetProjectViewDataDTO(
    IEnumerable<GetProjectViewTaskDTO> TaskList,
    IEnumerable<GetProjectViewAssignedUserDTO> AssignedUsers,
    GetProjectViewTopbarDTO Topbar);
