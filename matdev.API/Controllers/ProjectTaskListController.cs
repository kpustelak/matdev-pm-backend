using matdev.Application.DTOs.ProjectTaskList;
using matdev.Application.Interfaces;
using matdev.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace matdev.API.Controllers;

/// <summary>Full tasks list page (PDF layer 2b): paging, search, create task, mutations — separate from project dashboard <c>/view</c>.</summary>
[ApiController]
[Route("api/project/{projectId:int}/task-list")]
public class ProjectTaskListController : ControllerBase
{
    private readonly IProjectTaskListService _service;

    public ProjectTaskListController(IProjectTaskListService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResponseModel<GetProjectTaskListPageDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetProjectTaskListPageDTO>>> GetTaskList(
        [FromRoute] int projectId,
        [FromQuery] TaskListQueryParameters query)
    {
        var data = await _service.GetTaskListPageAsync(projectId, query);
        return Ok(new ResponseModel<GetProjectTaskListPageDTO> { Data = data, Message = "Task list loaded." });
    }

    [HttpGet("create-form")]
    [ProducesResponseType(typeof(ResponseModel<GetTaskCreateFormDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetTaskCreateFormDTO>>> GetCreateForm([FromRoute] int projectId)
    {
        var data = await _service.GetCreateFormAsync(projectId);
        return Ok(new ResponseModel<GetTaskCreateFormDTO> { Data = data, Message = "Create task form data loaded." });
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResponseModel<GetProjectTaskListItemDTO>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResponseModel<GetProjectTaskListItemDTO>>> CreateTask(
        [FromRoute] int projectId,
        [FromBody] CreateProjectTaskDTO dto)
    {
        var created = await _service.CreateTaskAsync(projectId, dto);
        return StatusCode(
            StatusCodes.Status201Created,
            new ResponseModel<GetProjectTaskListItemDTO> { Data = created, Message = "Task created." });
    }

    [HttpGet("{taskId:int}/subtasks")]
    [ProducesResponseType(typeof(ResponseModel<IReadOnlyList<GetProjectTaskListItemDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<GetProjectTaskListItemDTO>>>> GetSubtasks(
        [FromRoute] int projectId,
        [FromRoute] int taskId)
    {
        var data = await _service.GetSubtasksAsync(projectId, taskId);
        return Ok(new ResponseModel<IReadOnlyList<GetProjectTaskListItemDTO>> { Data = data, Message = "Subtasks loaded." });
    }

    [HttpPatch("{taskId:int}/status")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> ChangeTaskStatus(
        [FromRoute] int projectId,
        [FromRoute] int taskId,
        [FromBody] ChangeTaskStatusDTO dto)
    {
        await _service.ChangeTaskStatusAsync(projectId, taskId, dto);
        return Ok(new ResponseModel<object?> { Data = null, Message = "Task status changed." });
    }

    [HttpPatch("{taskId:int}/priority")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> ChangeTaskPriority(
        [FromRoute] int projectId,
        [FromRoute] int taskId,
        [FromBody] ChangeTaskPriorityDTO dto)
    {
        await _service.ChangeTaskPriorityAsync(projectId, taskId, dto);
        return Ok(new ResponseModel<object?> { Data = null, Message = "Task priority changed." });
    }

    [HttpPatch("{taskId:int}/deadline")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> ChangeTaskDeadline(
        [FromRoute] int projectId,
        [FromRoute] int taskId,
        [FromBody] ChangeTaskEndDateDTO dto)
    {
        await _service.ChangeTaskEndDateAsync(projectId, taskId, dto);
        return Ok(new ResponseModel<object?> { Data = null, Message = "Task deadline changed." });
    }

    [HttpPatch("{taskId:int}/sort-order")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> UpdateTaskSortOrder(
        [FromRoute] int projectId,
        [FromRoute] int taskId,
        [FromBody] UpdateTaskSortOrderDTO dto)
    {
        await _service.UpdateTaskSortOrderAsync(projectId, taskId, dto);
        return Ok(new ResponseModel<object?> { Data = null, Message = "Task sort order updated." });
    }

    [HttpDelete("{taskId:int}")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> DeleteTask([FromRoute] int projectId, [FromRoute] int taskId)
    {
        await _service.DeleteTaskAsync(projectId, taskId);
        return Ok(new ResponseModel<object?> { Data = null, Message = "Task deleted from project." });
    }
}
