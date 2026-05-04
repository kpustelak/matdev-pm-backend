using matdev.Domain.Entities;

namespace matdev.Domain.Interfaces;

public interface IProjectRepository : IRepository<Project>
{
    Task<Project?> GetByIdForUpdateAsync(int id);
}
