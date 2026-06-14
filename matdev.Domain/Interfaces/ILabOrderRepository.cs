using matdev.Domain.Entities.LabEntities;

namespace matdev.Domain.Interfaces;

public interface ILabOrderRepository
{
    Task<IReadOnlyList<LabOrder>> GetByProjectAsync(int projectId);
    Task<LabOrder?> GetByIdForProjectAsync(int projectId, int labOrderId);
    Task<IReadOnlyList<LabOrderStatus>> GetStatusesAsync();
    Task<LabOrderStatus?> GetStatusByIdAsync(int statusId);
    Task<LabOrder> AddAsync(LabOrder order, int projectId);
    Task UpdateAsync(LabOrder order);
    Task DeleteAsync(LabOrder order);
}
