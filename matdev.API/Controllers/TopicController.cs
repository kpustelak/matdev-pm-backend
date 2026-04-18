using matdev.Application.DTOs.Topic;
using matdev.Application.Interfaces;
using matdev.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace matdev.API.Controllers
{
    [ApiController]
    [Route("api/topic")]
    public class TopicController : ControllerBase
    {
       private readonly ITopicService _service;
        public TopicController(ITopicService service)
        {
            _service = service;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseModel<GetTopicDTO>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ResponseModel<GetTopicDTO>>> CreateTopic([FromBody] CreateTopicDTO dto)
        {
            var created = await _service.CreateAsync(dto);
            return StatusCode(
                StatusCodes.Status201Created,
                new ResponseModel<GetTopicDTO>
                {
                    Message = "Topic created successfully.",
                    Data = created
                }
                );
        }

        [HttpPatch]
        [ProducesResponseType(typeof(ResponseModel<GetTopicDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResponseModel<GetTopicDTO>>> UpdateTopic([FromBody] EditTopicDTO dto)
        {
            var updated = await _service.UpdateAsync(dto);
            return Ok(new ResponseModel<GetTopicDTO>
            {
                Message = "Topic updated successfully.",
                Data = updated
            });
        }

        [HttpGet]
        [ProducesResponseType(typeof(ResponseModel<IEnumerable<GetTopicDTO>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResponseModel<IEnumerable<GetTopicDTO>>>> GetTopics()
        {
            var topics = await _service.GetAllAsync();
            return Ok(new ResponseModel<IEnumerable<GetTopicDTO>>
            {
                Message = "Topics retrieved successfully.",
                Data = topics
            });
        }

        [HttpGet("id/{id}")]
        [ProducesResponseType(typeof(ResponseModel<GetTopicDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResponseModel<GetTopicDTO>>> GetTopicById([FromRoute] int id)
        {
            return Ok(new ResponseModel<GetTopicDTO>
            { Data = await _service.GetByIdAsync(id), Message = "The topic has been found." });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResponseModel<object?>>> DeleteTopic([FromRoute] int id)
        {
            await _service.DeleteAsync(id);
            return Ok(new ResponseModel<object?> { Message = "The topic has been deleted.", Data = null });
        }

        [HttpGet("search/{phrase}")]
        [ProducesResponseType(typeof(ResponseModel<IEnumerable<GetTopicDTO>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResponseModel<IEnumerable<GetTopicDTO>>>> SearchTopics([FromRoute] string phrase)
        {
            return Ok(new ResponseModel<IEnumerable<GetTopicDTO>>
            { Data = await _service.GetByPhraseAsync(phrase), Message = "Topics found." });
        }
    }
}
