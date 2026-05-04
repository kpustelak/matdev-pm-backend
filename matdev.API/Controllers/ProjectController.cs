using matdev.Application.DTOs.Project;
using matdev.Application.Interfaces;
using matdev.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace matdev.API.Controllers;

[ApiController]
[Route("api/project")]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _service;
    private readonly IProjectViewService _projectViewService;

    public ProjectController(IProjectService service, IProjectViewService projectViewService)
    {
        _service = service;
        _projectViewService = projectViewService;
    }

    /// <summary>Dropdown data for creating a project (IDs from DB — not tied to an existing project).</summary>
    [HttpGet("lookups/for-create")]
    [ProducesResponseType(typeof(ResponseModel<ProjectCreateLookupsDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<ProjectCreateLookupsDTO>>> GetCreateFormLookups()
    {
        var data = await _projectViewService.GetProjectCreateFormLookupsAsync();
        return Ok(new ResponseModel<ProjectCreateLookupsDTO> { Data = data, Message = "Lookups loaded." });
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResponseModel<GetProjectDTO>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResponseModel<GetProjectDTO>>> CreateProject([FromBody] CreateProjectDTO dto)
    {
        var created = await _service.CreateAsync(dto);
        return StatusCode(
            StatusCodes.Status201Created,
            new ResponseModel<GetProjectDTO> { Data = created, Message = "Project created successfully." });
    }

    [HttpPatch]
    [ProducesResponseType(typeof(ResponseModel<GetProjectDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetProjectDTO>>> UpdateProject([FromBody] EditProjectDTO dto)
    {
        var updated = await _service.UpdateAsync(dto);
        return Ok(new ResponseModel<GetProjectDTO> { Data = updated, Message = "Project updated successfully." });
    }

    [HttpGet("id/{id}")]
    [ProducesResponseType(typeof(ResponseModel<GetProjectDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetProjectDTO>>> GetProjectById([FromRoute] int id)
    {
        var project = await _service.GetByIdAsync(id);
        return Ok(new ResponseModel<GetProjectDTO> { Data = project, Message = "Project found." });
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResponseModel<IEnumerable<GetProjectDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<IEnumerable<GetProjectDTO>>>> GetAllProjects()
    {
        var projects = await _service.GetAllAsync();
        return Ok(new ResponseModel<IEnumerable<GetProjectDTO>> { Data = projects, Message = "Projects found." });
    }

    [HttpGet("search/{phrase}")]
    [ProducesResponseType(typeof(ResponseModel<IEnumerable<GetProjectDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<IEnumerable<GetProjectDTO>>>> SearchProjects([FromRoute] string phrase)
    {
        var projects = await _service.GetByPhraseAsync(phrase);
        return Ok(new ResponseModel<IEnumerable<GetProjectDTO>> { Data = projects, Message = "Projects found." });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> DeleteProject([FromRoute] int id)
    {
        await _service.DeleteAsync(id);
        return Ok(new ResponseModel<object?> { Data = null, Message = "Project deleted successfully." });
    }
}
