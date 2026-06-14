using matdev.Domain.Entities.LabEntities;
using matdev.Domain.Interfaces;
using matdev.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace matdev.Infrastructure.Repositories;

public class LabOrderRepository : ILabOrderRepository
{
    private readonly ApplicationDbContext _db;

    public LabOrderRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<LabOrder>> GetByProjectAsync(int projectId)
    {
        return await _db.LabOrderAssignments
            .Where(a => a.ProjectID == projectId)
            .Include(a => a.LabOrder)
                .ThenInclude(o => o.Status)
            .OrderByDescending(a => a.LabOrder.CreatedAt)
            .Select(a => a.LabOrder)
            .ToListAsync();
    }

    public async Task<LabOrder?> GetByIdForProjectAsync(int projectId, int labOrderId)
    {
        return await _db.LabOrderAssignments
            .Where(a => a.ProjectID == projectId && a.LabOrderID == labOrderId)
            .Include(a => a.LabOrder)
                .ThenInclude(o => o.Status)
            .Select(a => a.LabOrder)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<LabOrderStatus>> GetStatusesAsync()
    {
        return await _db.LabOrderStatuses
            .OrderBy(s => s.LabOrderStatusID)
            .ToListAsync();
    }

    public async Task<LabOrderStatus?> GetStatusByIdAsync(int statusId)
    {
        return await _db.LabOrderStatuses.FindAsync(statusId);
    }

    public async Task<LabOrder> AddAsync(LabOrder order, int projectId)
    {
        _db.LabOrders.Add(order);
        await _db.SaveChangesAsync();

        _db.LabOrderAssignments.Add(new LabOrderAssignment
        {
            LabOrderID = order.LabOrderID,
            ProjectID = projectId,
        });
        await _db.SaveChangesAsync();

        return order;
    }

    public async Task UpdateAsync(LabOrder order)
    {
        _db.LabOrders.Update(order);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(LabOrder order)
    {
        _db.LabOrders.Remove(order);
        await _db.SaveChangesAsync();
    }
}
