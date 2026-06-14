using matdev.Application.DTOs.ProjectView;
using matdev.Application.Interfaces;
using matdev.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace matdev.API.Controllers;

[ApiController]
[Route("api/project/{projectId:int}/view")]
public class ProjectViewController : ControllerBase
{
    private readonly IProjectViewService _service;

    public ProjectViewController(IProjectViewService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResponseModel<GetProjectViewDataDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetProjectViewDataDTO>>> GetProjectView([FromRoute] int projectId)
    {
        var data = await _service.GetProjectPageAsync(projectId);
        return Ok(new ResponseModel<GetProjectViewDataDTO> { Data = data, Message = "Project view found." });
    }

    [HttpGet("assignable-users")]
    [ProducesResponseType(typeof(ResponseModel<IEnumerable<GetProjectViewAssignedUserDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<IEnumerable<GetProjectViewAssignedUserDTO>>>> GetAssignableUsers([FromRoute] int projectId)
    {
        var users = await _service.GetAssignableUsersAsync(projectId);
        return Ok(new ResponseModel<IEnumerable<GetProjectViewAssignedUserDTO>> { Data = users, Message = "Assignable users found." });
    }

    [HttpGet("edit-project-form")]
    [ProducesResponseType(typeof(ResponseModel<IEnumerable<GetProjectViewEditProjectFormDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<IEnumerable<GetProjectViewEditProjectFormDTO>>>> GetEditProjectFormData([FromRoute] int projectId)
    {
        var data = await _service.GetEditProjectFormDataAsync(projectId);
        return Ok(new ResponseModel<IEnumerable<GetProjectViewEditProjectFormDTO>> { Data = data, Message = "Edit project form data found." });
    }

    [HttpPatch("status")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> ChangeProjectStatus([FromRoute] int projectId, [FromBody] ChangeProjectStatusDTO dto)
    {
        await _service.ChangeProjectStatusAsync(projectId, dto);
        return Ok(new ResponseModel<object?> { Data = null, Message = "Project status changed." });
    }

    [HttpPatch("deadline")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> ChangeProjectDeadline([FromRoute] int projectId, [FromBody] ChangeProjectDeadlineDTO dto)
    {
        await _service.ChangeProjectDeadlineAsync(projectId, dto);
        return Ok(new ResponseModel<object?> { Data = null, Message = "Project deadline changed." });
    }

    [HttpPost("assigned-users")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResponseModel<object?>>> AssignUser([FromRoute] int projectId, [FromBody] AssignUserToProjectDTO dto)
    {
        await _service.AssignUserAsync(projectId, dto);
        return StatusCode(StatusCodes.Status201Created, new ResponseModel<object?> { Data = null, Message = "User assigned to project." });
    }

    [HttpDelete("assigned-users/{userId:int}")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> RemoveUser([FromRoute] int projectId, [FromRoute] int userId)
    {
        await _service.RemoveUserAsync(projectId, userId);
        return Ok(new ResponseModel<object?> { Data = null, Message = "User removed from project." });
    }
}
