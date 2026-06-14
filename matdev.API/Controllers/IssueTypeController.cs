using matdev.Application.DTOs.IssueType;
using matdev.Application.DTOs.User;
using matdev.Application.Interfaces;
using matdev.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace matdev.API.Controllers
{
    [ApiController]
    [Route("api/issuetype")]
    public class IssueTypeController : ControllerBase
    {
       private readonly IIssueTypeService _service;
        public IssueTypeController(IIssueTypeService service)
        {
            _service = service;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseModel<GetIssueTypeDTO>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ResponseModel<GetIssueTypeDTO>>> CreateIssueType([FromBody] CreateIssueTypeDTO dto)
        {
            var created = await _service.CreateAsync(dto);
            return StatusCode(
                StatusCodes.Status201Created,
                new ResponseModel<GetIssueTypeDTO>
                {
                    Message = "Issue type created successfully.",
                    Data = created
                }
                );
        }

        [HttpPatch]
        [ProducesResponseType(typeof(ResponseModel<GetIssueTypeDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResponseModel<GetIssueTypeDTO>>> UpdateIssueType([FromBody] EditIssueTypeDTO dto)
        {
            var updated = await _service.UpdateAsync(dto);
            return Ok(new ResponseModel<GetIssueTypeDTO>
            {
                Message = "Issue type updated successfully.",
                Data = updated
            });
        }

        [HttpGet]
        [ProducesResponseType(typeof(ResponseModel<IEnumerable<GetIssueTypeDTO>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResponseModel<IEnumerable<GetIssueTypeDTO>>>> GetIssueTypes()
        {
            var issueTypes = await _service.GetAllAsync();
            return Ok(new ResponseModel<IEnumerable<GetIssueTypeDTO>>
            {
                Message = "Issue types retrieved successfully.",
                Data = issueTypes
            });
        }

        [HttpGet("id/{id}")]
        [ProducesResponseType(typeof(ResponseModel<GetIssueTypeDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResponseModel<GetIssueTypeDTO>>> GetIssueTypeById([FromRoute] int id)
        {
            return Ok(new ResponseModel<GetIssueTypeDTO>
            { Data = await _service.GetByIdAsync(id), Message = "The issue type has been found." });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResponseModel<object?>>> DeleteIssueType([FromRoute] int id)
        {
            await _service.DeleteAsync(id);
            return Ok(new ResponseModel<object?> { Message = "The issue type has been deleted.", Data = null });
        }

        [HttpGet("search/{phrase}")]
        [ProducesResponseType(typeof(ResponseModel<IEnumerable<GetIssueTypeDTO>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResponseModel<IEnumerable<GetIssueTypeDTO>>>> SearchIssueTypes([FromRoute] string phrase)
        {
            return Ok(new ResponseModel<IEnumerable<GetIssueTypeDTO>>
            { Data = await _service.GetByPhraseAsync(phrase), Message = "Issue types found." });
        }
    }
}
