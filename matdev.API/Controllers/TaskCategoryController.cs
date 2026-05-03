using matdev.Application.DTOs.TaskCategory;
using matdev.Application.Interfaces;
using matdev.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace matdev.API.Controllers;

[ApiController]
[Route("api/taskcategory")]
public class TaskCategoryController : ControllerBase
{
    private readonly ITaskCategoryService _service;

    public TaskCategoryController(ITaskCategoryService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResponseModel<GetTaskCategoryDTO>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResponseModel<GetTaskCategoryDTO>>> CreateTaskCategory([FromBody] CreateTaskCategoryDTO dto)
    {
        var created = await _service.CreateAsync(dto);
        return StatusCode(
            StatusCodes.Status201Created,
            new ResponseModel<GetTaskCategoryDTO>
            {
                Message = "Task category created successfully.",
                Data = created
            });
    }

    [HttpPatch]
    [ProducesResponseType(typeof(ResponseModel<GetTaskCategoryDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetTaskCategoryDTO>>> UpdateTaskCategory([FromBody] EditTaskCategoryDTO dto)
    {
        var updated = await _service.UpdateAsync(dto);
        return Ok(new ResponseModel<GetTaskCategoryDTO>
        {
            Message = "Task category updated successfully.",
            Data = updated
        });
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResponseModel<IEnumerable<GetTaskCategoryDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<IEnumerable<GetTaskCategoryDTO>>>> GetTaskCategories()
    {
        var list = await _service.GetAllAsync();
        return Ok(new ResponseModel<IEnumerable<GetTaskCategoryDTO>>
        {
            Message = "Task categories retrieved successfully.",
            Data = list
        });
    }

    [HttpGet("id/{id}")]
    [ProducesResponseType(typeof(ResponseModel<GetTaskCategoryDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetTaskCategoryDTO>>> GetTaskCategoryById([FromRoute] int id)
    {
        return Ok(new ResponseModel<GetTaskCategoryDTO>
        {
            Data = await _service.GetByIdAsync(id),
            Message = "The task category has been found."
        });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> DeleteTaskCategory([FromRoute] int id)
    {
        await _service.DeleteAsync(id);
        return Ok(new ResponseModel<object?> { Message = "The task category has been deleted.", Data = null });
    }

    [HttpGet("search/{phrase}")]
    [ProducesResponseType(typeof(ResponseModel<IEnumerable<GetTaskCategoryDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<IEnumerable<GetTaskCategoryDTO>>>> SearchTaskCategories([FromRoute] string phrase)
    {
        return Ok(new ResponseModel<IEnumerable<GetTaskCategoryDTO>>
        {
            Data = await _service.GetByPhraseAsync(phrase),
            Message = "Task categories found."
        });
    }
}
