using matdev.Application.DTOs.TaskView;

namespace matdev.Application.Interfaces;

public interface ITaskViewService
{
    Task<GetTaskViewDTO> GetTaskViewAsync(int projectId, int taskId);

    Task ChangeTaskStatusAsync(int projectId, int taskId, ChangeTaskViewStatusDTO dto);
    Task ChangeTaskDeadlineAsync(int projectId, int taskId, ChangeTaskViewDeadlineDTO dto);
    Task ChangeTaskPriorityAsync(int projectId, int taskId, ChangeTaskViewPriorityDTO dto);

    Task<GetCreateSubtaskFormDTO> GetCreateSubtaskFormAsync(int projectId);
    Task<GetTaskViewSubtaskDTO> CreateSubtaskAsync(int projectId, int parentTaskId, CreateSubtaskDTO dto);
    Task DeleteSubtaskAsync(int projectId, int subtaskId);
    Task ChangeSubtaskStatusAsync(int projectId, int subtaskId, ChangeSubtaskStatusDTO dto);
    Task ChangeSubtaskStartDateAsync(int projectId, int subtaskId, ChangeSubtaskStartDateDTO dto);

    Task<GetTaskEditFormDTO> GetEditFormAsync(int projectId);
    Task<GetTaskViewDTO> EditTaskAsync(int projectId, int taskId, EditTaskDTO dto);

    Task<IReadOnlyList<GetTaskViewAssignedUserDTO>> GetTaskAssignmentsAsync(int projectId, int taskId);
    Task AssignUserToTaskAsync(int projectId, int taskId, AssignUserToTaskDTO dto);
    Task RemoveUserFromTaskAsync(int projectId, int taskId, int userId);
}
