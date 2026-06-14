namespace matdev.Application.DTOs.TaskView;

public record GetTaskViewDTO(
    GetTaskViewTopbarDTO Topbar,
    IReadOnlyList<GetTaskViewSubtaskDTO> Subtasks,
    IReadOnlyList<GetTaskViewAssignedUserDTO> Assignments,
    GetTaskViewCostsDTO Costs
);
