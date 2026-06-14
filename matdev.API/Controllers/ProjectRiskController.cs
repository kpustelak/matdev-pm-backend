using matdev.Application.DTOs.Risk;
using matdev.Application.Interfaces;
using matdev.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace matdev.API.Controllers;

[ApiController]
[Route("api/project/{projectId:int}/risks")]
public class ProjectRiskController : ControllerBase
{
    private readonly IProjectRiskService _service;

    public ProjectRiskController(IProjectRiskService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResponseModel<IReadOnlyList<GetProjectRiskDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<GetProjectRiskDTO>>>> GetRisks(
        [FromRoute] int projectId)
    {
        var data = await _service.GetByProjectAsync(projectId);
        return Ok(new ResponseModel<IReadOnlyList<GetProjectRiskDTO>> { Data = data, Message = "Risks loaded." });
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResponseModel<GetProjectRiskDTO>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResponseModel<GetProjectRiskDTO>>> CreateRisk(
        [FromRoute] int projectId,
        [FromBody] CreateProjectRiskDTO dto)
    {
        var created = await _service.CreateAsync(projectId, dto);
        return StatusCode(StatusCodes.Status201Created,
            new ResponseModel<GetProjectRiskDTO> { Data = created, Message = "Risk created." });
    }

    [HttpPatch("{riskId:int}/resolve")]
    [ProducesResponseType(typeof(ResponseModel<GetProjectRiskDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetProjectRiskDTO>>> ResolveRisk(
        [FromRoute] int projectId,
        [FromRoute] int riskId)
    {
        var updated = await _service.ResolveAsync(projectId, riskId);
        return Ok(new ResponseModel<GetProjectRiskDTO> { Data = updated, Message = "Risk resolved." });
    }

    [HttpDelete("{riskId:int}")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> DeleteRisk(
        [FromRoute] int projectId,
        [FromRoute] int riskId)
    {
        await _service.DeleteAsync(projectId, riskId);
        return Ok(new ResponseModel<object?> { Data = null, Message = "Risk deleted." });
    }
}
