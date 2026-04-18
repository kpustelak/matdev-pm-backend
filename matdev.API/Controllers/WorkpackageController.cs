using matdev.Application.DTOs.Workpackage;
using matdev.Application.Interfaces;
using matdev.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace matdev.API.Controllers
{
    [ApiController]
    [Route("api/workpackage")]
    public class WorkpackageController : ControllerBase
    {
       private readonly IWorkpackageService _service;
        public WorkpackageController(IWorkpackageService service)
        {
            _service = service;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseModel<GetWorkpackageDTO>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ResponseModel<GetWorkpackageDTO>>> CreateWorkpackage([FromBody] CreateWorkpackageDTO dto)
        {
            var created = await _service.CreateAsync(dto);
            return StatusCode(
                StatusCodes.Status201Created,
                new ResponseModel<GetWorkpackageDTO>
                {
                    Message = "Workpackage created successfully.",
                    Data = created
                }
                );
        }

        [HttpPatch]
        [ProducesResponseType(typeof(ResponseModel<GetWorkpackageDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResponseModel<GetWorkpackageDTO>>> UpdateWorkpackage([FromBody] EditWorkpackageDTO dto)
        {
            var updated = await _service.UpdateAsync(dto);
            return Ok(new ResponseModel<GetWorkpackageDTO>
            {
                Message = "Workpackage updated successfully.",
                Data = updated
            });
        }

        [HttpGet]
        [ProducesResponseType(typeof(ResponseModel<IEnumerable<GetWorkpackageDTO>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResponseModel<IEnumerable<GetWorkpackageDTO>>>> GetWorkpackages()
        {
            var workpackages = await _service.GetAllAsync();
            return Ok(new ResponseModel<IEnumerable<GetWorkpackageDTO>>
            {
                Message = "Workpackages retrieved successfully.",
                Data = workpackages
            });
        }

        [HttpGet("id/{id}")]
        [ProducesResponseType(typeof(ResponseModel<GetWorkpackageDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResponseModel<GetWorkpackageDTO>>> GetWorkpackageById([FromRoute] int id)
        {
            return Ok(new ResponseModel<GetWorkpackageDTO>
            { Data = await _service.GetByIdAsync(id), Message = "The workpackage has been found." });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResponseModel<object?>>> DeleteWorkpackage([FromRoute] int id)
        {
            await _service.DeleteAsync(id);
            return Ok(new ResponseModel<object?> { Message = "The workpackage has been deleted.", Data = null });
        }

        [HttpGet("search/{phrase}")]
        [ProducesResponseType(typeof(ResponseModel<IEnumerable<GetWorkpackageDTO>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResponseModel<IEnumerable<GetWorkpackageDTO>>>> SearchWorkpackages([FromRoute] string phrase)
        {
            return Ok(new ResponseModel<IEnumerable<GetWorkpackageDTO>>
            { Data = await _service.GetByPhraseAsync(phrase), Message = "Workpackages found." });
        }
    }
}
