using matdev.Application.DTOs.ProjectTaskList;

namespace matdev.Application.Interfaces;

public interface IProjectTaskListService
{
    Task<GetProjectTaskListPageDTO> GetTaskListPageAsync(int projectId, TaskListQueryParameters query);

    Task<IReadOnlyList<GetProjectTaskListItemDTO>> GetSubtasksAsync(int projectId, int parentTaskId);

    Task<GetTaskCreateFormDTO> GetCreateFormAsync(int projectId);

    Task<GetProjectTaskListItemDTO> CreateTaskAsync(int projectId, CreateProjectTaskDTO dto);

    Task ChangeTaskStatusAsync(int projectId, int taskId, ChangeTaskStatusDTO dto);

    Task ChangeTaskPriorityAsync(int projectId, int taskId, ChangeTaskPriorityDTO dto);

    Task ChangeTaskEndDateAsync(int projectId, int taskId, ChangeTaskEndDateDTO dto);

    Task UpdateTaskSortOrderAsync(int projectId, int taskId, UpdateTaskSortOrderDTO dto);

    Task DeleteTaskAsync(int projectId, int taskId);
}
