using matdev.Application.DTOs.LabOrder;
using matdev.Application.Interfaces;
using matdev.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace matdev.API.Controllers;

[ApiController]
[Route("api/project/{projectId:int}/lab-orders")]
public class LabOrderController : ControllerBase
{
    private const long MaxReportBytes = 52_428_800;

    private readonly ILabOrderService _service;

    public LabOrderController(ILabOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResponseModel<IReadOnlyList<GetLabOrderDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<GetLabOrderDTO>>>> GetLabOrders(
        [FromRoute] int projectId)
    {
        var data = await _service.GetByProjectAsync(projectId);
        return Ok(new ResponseModel<IReadOnlyList<GetLabOrderDTO>> { Data = data, Message = "Lab orders loaded." });
    }

    [HttpGet("statuses")]
    [ProducesResponseType(typeof(ResponseModel<IReadOnlyList<GetLabOrderStatusDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<GetLabOrderStatusDTO>>>> GetStatuses()
    {
        var data = await _service.GetStatusesAsync();
        return Ok(new ResponseModel<IReadOnlyList<GetLabOrderStatusDTO>> { Data = data, Message = "Lab order statuses loaded." });
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResponseModel<GetLabOrderDTO>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResponseModel<GetLabOrderDTO>>> CreateLabOrder(
        [FromRoute] int projectId,
        [FromBody] CreateLabOrderDTO dto)
    {
        var created = await _service.CreateAsync(projectId, dto);
        return StatusCode(StatusCodes.Status201Created,
            new ResponseModel<GetLabOrderDTO> { Data = created, Message = "Lab order created." });
    }

    [HttpPut("{labOrderId:int}")]
    [ProducesResponseType(typeof(ResponseModel<GetLabOrderDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetLabOrderDTO>>> UpdateLabOrder(
        [FromRoute] int projectId,
        [FromRoute] int labOrderId,
        [FromBody] UpdateLabOrderDTO dto)
    {
        var updated = await _service.UpdateAsync(projectId, labOrderId, dto);
        return Ok(new ResponseModel<GetLabOrderDTO> { Data = updated, Message = "Lab order updated." });
    }

    [HttpDelete("{labOrderId:int}")]
    [ProducesResponseType(typeof(ResponseModel<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<object?>>> DeleteLabOrder(
        [FromRoute] int projectId,
        [FromRoute] int labOrderId)
    {
        await _service.DeleteAsync(projectId, labOrderId);
        return Ok(new ResponseModel<object?> { Data = null, Message = "Lab order deleted." });
    }

    [HttpPost("{labOrderId:int}/reports/test")]
    [RequestSizeLimit(MaxReportBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxReportBytes)]
    [ProducesResponseType(typeof(ResponseModel<GetLabOrderDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetLabOrderDTO>>> UploadTestReport(
        [FromRoute] int projectId,
        [FromRoute] int labOrderId,
        IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ResponseModel<object?> { Data = null, Message = "File is required." });

        await using var stream = file.OpenReadStream();
        var updated = await _service.UploadTestReportAsync(projectId, labOrderId, stream, file.FileName);
        return Ok(new ResponseModel<GetLabOrderDTO> { Data = updated, Message = "Test report uploaded." });
    }

    [HttpPost("{labOrderId:int}/reports/final")]
    [RequestSizeLimit(MaxReportBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxReportBytes)]
    [ProducesResponseType(typeof(ResponseModel<GetLabOrderDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetLabOrderDTO>>> UploadFinalReport(
        [FromRoute] int projectId,
        [FromRoute] int labOrderId,
        IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ResponseModel<object?> { Data = null, Message = "File is required." });

        await using var stream = file.OpenReadStream();
        var updated = await _service.UploadFinalReportAsync(projectId, labOrderId, stream, file.FileName);
        return Ok(new ResponseModel<GetLabOrderDTO> { Data = updated, Message = "Final report uploaded." });
    }

    [HttpGet("{labOrderId:int}/reports/test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadTestReport([FromRoute] int projectId, [FromRoute] int labOrderId)
    {
        var file = await _service.GetTestReportFileAsync(projectId, labOrderId);
        if (file is null)
            return NotFound();

        return File(file.Value.Stream, file.Value.ContentType, file.Value.FileName);
    }

    [HttpGet("{labOrderId:int}/reports/final")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadFinalReport([FromRoute] int projectId, [FromRoute] int labOrderId)
    {
        var file = await _service.GetFinalReportFileAsync(projectId, labOrderId);
        if (file is null)
            return NotFound();

        return File(file.Value.Stream, file.Value.ContentType, file.Value.FileName);
    }

    [HttpDelete("{labOrderId:int}/reports/test")]
    [ProducesResponseType(typeof(ResponseModel<GetLabOrderDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetLabOrderDTO>>> DeleteTestReport(
        [FromRoute] int projectId,
        [FromRoute] int labOrderId)
    {
        var updated = await _service.DeleteTestReportAsync(projectId, labOrderId);
        return Ok(new ResponseModel<GetLabOrderDTO> { Data = updated, Message = "Test report removed." });
    }

    [HttpDelete("{labOrderId:int}/reports/final")]
    [ProducesResponseType(typeof(ResponseModel<GetLabOrderDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseModel<GetLabOrderDTO>>> DeleteFinalReport(
        [FromRoute] int projectId,
        [FromRoute] int labOrderId)
    {
        var updated = await _service.DeleteFinalReportAsync(projectId, labOrderId);
        return Ok(new ResponseModel<GetLabOrderDTO> { Data = updated, Message = "Final report removed." });
    }
}
