using matdev.Application.DTOs.LabOrder;

namespace matdev.Application.Interfaces;

public interface ILabOrderService
{
    Task<IReadOnlyList<GetLabOrderDTO>> GetByProjectAsync(int projectId);
    Task<IReadOnlyList<GetLabOrderStatusDTO>> GetStatusesAsync();
    Task<GetLabOrderDTO> CreateAsync(int projectId, CreateLabOrderDTO dto);
    Task<GetLabOrderDTO> UpdateAsync(int projectId, int labOrderId, UpdateLabOrderDTO dto);
    Task DeleteAsync(int projectId, int labOrderId);

    Task<GetLabOrderDTO> UploadTestReportAsync(int projectId, int labOrderId, Stream content, string fileName);
    Task<GetLabOrderDTO> UploadFinalReportAsync(int projectId, int labOrderId, Stream content, string fileName);
    Task<(Stream Stream, string FileName, string ContentType)?> GetTestReportFileAsync(int projectId, int labOrderId);
    Task<(Stream Stream, string FileName, string ContentType)?> GetFinalReportFileAsync(int projectId, int labOrderId);
    Task<GetLabOrderDTO> DeleteTestReportAsync(int projectId, int labOrderId);
    Task<GetLabOrderDTO> DeleteFinalReportAsync(int projectId, int labOrderId);
}
