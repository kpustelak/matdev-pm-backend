using matdev.Application.DTOs.TaskView;
using matdev.Application.Interfaces;
using matdev.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace matdev.API.Controllers;

[ApiController]
[Route("api/project/{projectId:int}/task/{taskId:int}/view")]
public class TaskViewController : ControllerBase
{
    private readonly ITaskViewService _service;

    public TaskViewController(ITaskViewService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResponseModel<GetTaskViewDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetTaskViewDTO>>> GetTaskView(
        [FromRoute] int projectId,
        [FromRoute] int taskId)
    {
        var data = await _service.GetTaskViewAsync(projectId, taskId);
        return Ok(new ResponseModel<GetTaskViewDTO> { Data = data, Message = "Task view loaded." });
    }

    [HttpPatch("status")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> ChangeTaskStatus(
        [FromRoute] int projectId,
        [FromRoute] int taskId,
        [FromBody] ChangeTaskViewStatusDTO dto)
    {
        await _service.ChangeTaskStatusAsync(projectId, taskId, dto);
        return Ok(new ResponseModel<object?> { Data = null, Message = "Task status changed." });
    }

    [HttpPatch("deadline")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> ChangeTaskDeadline(
        [FromRoute] int projectId,
        [FromRoute] int taskId,
        [FromBody] ChangeTaskViewDeadlineDTO dto)
    {
        await _service.ChangeTaskDeadlineAsync(projectId, taskId, dto);
        return Ok(new ResponseModel<object?> { Data = null, Message = "Task deadline changed." });
    }

    [HttpPatch("priority")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> ChangeTaskPriority(
        [FromRoute] int projectId,
        [FromRoute] int taskId,
        [FromBody] ChangeTaskViewPriorityDTO dto)
    {
        await _service.ChangeTaskPriorityAsync(projectId, taskId, dto);
        return Ok(new ResponseModel<object?> { Data = null, Message = "Task priority changed." });
    }

    [HttpGet("create-subtask-form")]
    [ProducesResponseType(typeof(ResponseModel<GetCreateSubtaskFormDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetCreateSubtaskFormDTO>>> GetCreateSubtaskForm(
        [FromRoute] int projectId,
        [FromRoute] int taskId)
    {
        var data = await _service.GetCreateSubtaskFormAsync(projectId);
        return Ok(new ResponseModel<GetCreateSubtaskFormDTO> { Data = data, Message = "Create subtask form data loaded." });
    }

    [HttpPost("subtasks")]
    [ProducesResponseType(typeof(ResponseModel<GetTaskViewSubtaskDTO>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResponseModel<GetTaskViewSubtaskDTO>>> CreateSubtask(
        [FromRoute] int projectId,
        [FromRoute] int taskId,
        [FromBody] CreateSubtaskDTO dto)
    {
        var created = await _service.CreateSubtaskAsync(projectId, taskId, dto);
        return StatusCode(
            StatusCodes.Status201Created,
            new ResponseModel<GetTaskViewSubtaskDTO> { Data = created, Message = "Subtask created." });
    }

    [HttpDelete("subtasks/{subtaskId:int}")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> DeleteSubtask(
        [FromRoute] int projectId,
        [FromRoute] int taskId,
        [FromRoute] int subtaskId)
    {
        await _service.DeleteSubtaskAsync(projectId, subtaskId);
        return Ok(new ResponseModel<object?> { Data = null, Message = "Subtask deleted." });
    }

    [HttpPatch("subtasks/{subtaskId:int}/status")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> ChangeSubtaskStatus(
        [FromRoute] int projectId,
        [FromRoute] int taskId,
        [FromRoute] int subtaskId,
        [FromBody] ChangeSubtaskStatusDTO dto)
    {
        await _service.ChangeSubtaskStatusAsync(projectId, subtaskId, dto);
        return Ok(new ResponseModel<object?> { Data = null, Message = "Subtask status changed." });
    }

    [HttpPatch("subtasks/{subtaskId:int}/start-date")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> ChangeSubtaskStartDate(
        [FromRoute] int projectId,
        [FromRoute] int taskId,
        [FromRoute] int subtaskId,
        [FromBody] ChangeSubtaskStartDateDTO dto)
    {
        await _service.ChangeSubtaskStartDateAsync(projectId, subtaskId, dto);
        return Ok(new ResponseModel<object?> { Data = null, Message = "Subtask start date changed." });
    }

    [HttpPatch("subtasks/{subtaskId:int}/end-date")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> ChangeSubtaskEndDate(
        [FromRoute] int projectId,
        [FromRoute] int taskId,
        [FromRoute] int subtaskId,
        [FromBody] ChangeSubtaskEndDateDTO dto)
    {
        await _service.ChangeSubtaskEndDateAsync(projectId, subtaskId, dto);
        return Ok(new ResponseModel<object?> { Data = null, Message = "Subtask end date changed." });
    }

    [HttpGet("edit-form")]
    [ProducesResponseType(typeof(ResponseModel<GetTaskEditFormDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetTaskEditFormDTO>>> GetEditForm(
        [FromRoute] int projectId,
        [FromRoute] int taskId)
    {
        var data = await _service.GetEditFormAsync(projectId);
        return Ok(new ResponseModel<GetTaskEditFormDTO> { Data = data, Message = "Edit task form data loaded." });
    }

    [HttpPut]
    [ProducesResponseType(typeof(ResponseModel<GetTaskViewDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetTaskViewDTO>>> EditTask(
        [FromRoute] int projectId,
        [FromRoute] int taskId,
        [FromBody] EditTaskDTO dto)
    {
        var updated = await _service.EditTaskAsync(projectId, taskId, dto);
        return Ok(new ResponseModel<GetTaskViewDTO> { Data = updated, Message = "Task updated." });
    }

    [HttpGet("assignments")]
    [ProducesResponseType(typeof(ResponseModel<IReadOnlyList<GetTaskViewAssignedUserDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<GetTaskViewAssignedUserDTO>>>> GetTaskAssignments(
        [FromRoute] int projectId,
        [FromRoute] int taskId)
    {
        var data = await _service.GetTaskAssignmentsAsync(projectId, taskId);
        return Ok(new ResponseModel<IReadOnlyList<GetTaskViewAssignedUserDTO>> { Data = data, Message = "Task assignments loaded." });
    }

    [HttpPost("assignments")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResponseModel<object?>>> AssignUserToTask(
        [FromRoute] int projectId,
        [FromRoute] int taskId,
        [FromBody] AssignUserToTaskDTO dto)
    {
        await _service.AssignUserToTaskAsync(projectId, taskId, dto);
        return StatusCode(
            StatusCodes.Status201Created,
            new ResponseModel<object?> { Data = null, Message = "User assigned to task." });
    }

    [HttpDelete("assignments/{userId:int}")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> RemoveUserFromTask(
        [FromRoute] int projectId,
        [FromRoute] int taskId,
        [FromRoute] int userId)
    {
        await _service.RemoveUserFromTaskAsync(projectId, taskId, userId);
        return Ok(new ResponseModel<object?> { Data = null, Message = "User removed from task." });
    }
}
