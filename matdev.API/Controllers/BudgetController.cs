using matdev.Application.DTOs.Budget;
using matdev.Application.Interfaces;
using matdev.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace matdev.API.Controllers;

[ApiController]
[Route("api/project/{projectId:int}/budget")]
public class BudgetController : ControllerBase
{
    private readonly IBudgetService _service;

    public BudgetController(IBudgetService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResponseModel<GetProjectBudgetDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseModel<GetProjectBudgetDTO>>> GetProjectBudget(
        [FromRoute] int projectId)
    {
        var data = await _service.GetProjectBudgetAsync(projectId);
        if (data is null)
            return NotFound(new ResponseModel<object?> { Data = null, Message = "No budget plan found for this project." });

        return Ok(new ResponseModel<GetProjectBudgetDTO> { Data = data, Message = "Budget loaded." });
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(ResponseModel<IReadOnlyList<BudgetCategoryDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<BudgetCategoryDTO>>>> GetBudgetCategories(
        [FromRoute] int projectId)
    {
        var data = await _service.GetBudgetCategoriesAsync();
        return Ok(new ResponseModel<IReadOnlyList<BudgetCategoryDTO>> { Data = data, Message = "Budget categories loaded." });
    }

    [HttpPost("categories")]
    [ProducesResponseType(typeof(ResponseModel<BudgetCategoryDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ResponseModel<BudgetCategoryDTO>>> CreateBudgetCategory(
        [FromRoute] int projectId,
        [FromBody] CreateBudgetCategoryDTO dto)
    {
        var data = await _service.CreateBudgetCategoryAsync(dto);
        return StatusCode(
            StatusCodes.Status201Created,
            new ResponseModel<BudgetCategoryDTO> { Data = data, Message = "Budget category created." });
    }

    [HttpGet("lines")]
    [ProducesResponseType(typeof(ResponseModel<IReadOnlyList<BudgetPlanLineDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<BudgetPlanLineDTO>>>> GetBudgetLines(
        [FromRoute] int projectId)
    {
        var data = await _service.GetBudgetLinesAsync(projectId);
        return Ok(new ResponseModel<IReadOnlyList<BudgetPlanLineDTO>> { Data = data, Message = "Budget lines loaded." });
    }

    [HttpPut("lines")]
    [ProducesResponseType(typeof(ResponseModel<IReadOnlyList<BudgetPlanLineDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<BudgetPlanLineDTO>>>> ReplaceBudgetLines(
        [FromRoute] int projectId,
        [FromBody] UpdateBudgetLinesDTO dto)
    {
        var data = await _service.ReplaceBudgetLinesAsync(projectId, dto);
        return Ok(new ResponseModel<IReadOnlyList<BudgetPlanLineDTO>> { Data = data, Message = "Budget allocations saved." });
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResponseModel<GetProjectBudgetDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ResponseModel<GetProjectBudgetDTO>>> CreateBudgetPlan(
        [FromRoute] int projectId,
        [FromBody] CreateBudgetPlanDTO dto)
    {
        var data = await _service.CreateBudgetPlanAsync(projectId, dto);
        return StatusCode(
            StatusCodes.Status201Created,
            new ResponseModel<GetProjectBudgetDTO> { Data = data, Message = "Budget plan created." });
    }

    [HttpPut]
    [ProducesResponseType(typeof(ResponseModel<GetProjectBudgetDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseModel<GetProjectBudgetDTO>>> UpdateBudgetPlan(
        [FromRoute] int projectId,
        [FromBody] UpdateBudgetPlanDTO dto)
    {
        var data = await _service.UpdateBudgetPlanAsync(projectId, dto);
        if (data is null)
            return NotFound(new ResponseModel<object?> { Data = null, Message = "No budget plan found for this project." });

        return Ok(new ResponseModel<GetProjectBudgetDTO> { Data = data, Message = "Budget plan updated." });
    }

    [HttpPost("expenditures")]
    [ProducesResponseType(typeof(ResponseModel<GetProjectBudgetDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseModel<GetProjectBudgetDTO>>> AddExpenditure(
        [FromRoute] int projectId,
        [FromBody] CreateExpenditureDTO dto)
    {
        var data = await _service.AddExpenditureAsync(projectId, dto);
        if (data is null)
            return NotFound(new ResponseModel<object?> { Data = null, Message = "No budget plan found for this project." });

        return StatusCode(StatusCodes.Status201Created,
            new ResponseModel<GetProjectBudgetDTO> { Data = data, Message = "Expenditure added." });
    }

    [HttpPut("expenditures/{expenditureId:int}")]
    [ProducesResponseType(typeof(ResponseModel<GetProjectBudgetDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseModel<GetProjectBudgetDTO>>> UpdateExpenditure(
        [FromRoute] int projectId,
        [FromRoute] int expenditureId,
        [FromBody] UpdateExpenditureDTO dto)
    {
        var data = await _service.UpdateExpenditureAsync(projectId, expenditureId, dto);
        return Ok(new ResponseModel<GetProjectBudgetDTO> { Data = data, Message = "Expenditure updated." });
    }

    [HttpDelete("expenditures/{expenditureId:int}")]
    [ProducesResponseType(typeof(ResponseModel<GetProjectBudgetDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetProjectBudgetDTO>>> DeleteExpenditure(
        [FromRoute] int projectId,
        [FromRoute] int expenditureId)
    {
        var data = await _service.DeleteExpenditureAsync(projectId, expenditureId);
        return Ok(new ResponseModel<GetProjectBudgetDTO> { Data = data, Message = "Expenditure deleted." });
    }
}
