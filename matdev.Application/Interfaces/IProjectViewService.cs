using matdev.Application.DTOs.ProjectView;

namespace matdev.Application.Interfaces;

public interface IProjectViewService
{
    Task<GetProjectViewDataDTO> GetProjectPageAsync(int projectId);
    Task<IEnumerable<GetProjectViewAssignedUserDTO>> GetAssignableUsersAsync(int projectId);
    Task<IEnumerable<GetProjectViewEditProjectFormDTO>> GetEditProjectFormDataAsync(int projectId);
    Task ChangeProjectStatusAsync(int projectId, ChangeProjectStatusDTO dto);
    Task ChangeProjectDeadlineAsync(int projectId, ChangeProjectDeadlineDTO dto);
    Task AssignUserAsync(int projectId, AssignUserToProjectDTO dto);
    Task RemoveUserAsync(int projectId, int userId);
    Task DeleteTaskAsync(int projectId, int taskId);
}
