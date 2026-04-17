using matdev.Application.DTOs.User;
using matdev.Application.Interfaces;
using matdev.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace matdev.API.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(ResponseModel<GetUserDTO>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResponseModel<GetUserDTO>>> CreateUser([FromBody] CreateUserDTO dto)
    {
        var created = await _service.CreateAsync(dto);
        return StatusCode(
            StatusCodes.Status201Created,
            new ResponseModel<GetUserDTO> { Data = created, Message = "User created." });
    }

    [HttpPatch("edit")]
    [ProducesResponseType(typeof(ResponseModel<GetUserDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetUserDTO>>> EditUser([FromBody] EditUserDTO dto)
    {
        return Ok(new ResponseModel<GetUserDTO> 
            { Data = await _service.UpdateAsync(dto), Message = "User updated." });
    }

    [HttpGet("id/{id}")]
    [ProducesResponseType(typeof(ResponseModel<GetUserDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetUserDTO>>> GetUserById([FromRoute] int id)
    {
        return Ok(new ResponseModel<GetUserDTO>
            { Data = await _service.GetByIdAsync(id), Message = "The user has been found." });
    }

    [HttpGet("search/{phrase}")]
    [ProducesResponseType(typeof(ResponseModel<IEnumerable<GetUserDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<IEnumerable<GetUserDTO>>>> SearchUsers([FromRoute] string phrase)
    {
        return Ok(new ResponseModel<IEnumerable<GetUserDTO>>
            { Data = await _service.GetByPhraseAsync(phrase), Message = "Users found." });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> DeleteUser([FromRoute] int id)
    {
        await _service.DeleteAsync(id);
        return Ok(new ResponseModel<object?> { Message = "User has been deleted.", Data = null });
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResponseModel<IEnumerable<GetUserDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<IEnumerable<GetUserDTO>>>> GetAllUsers()
    {
        return Ok(new ResponseModel<IEnumerable<GetUserDTO>>
            { Data = await _service.GetAllAsync(), Message = "Users found." });
    }
}
