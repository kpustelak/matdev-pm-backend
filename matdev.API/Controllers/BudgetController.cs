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
