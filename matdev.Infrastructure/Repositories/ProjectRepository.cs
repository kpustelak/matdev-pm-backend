using matdev.Domain.Entities;
using matdev.Domain.Interfaces;
using matdev.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace matdev.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext _context;

    public ProjectRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        return await _context.Projects.FindAsync(id);
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        return await _context.Projects.ToListAsync();
    }

    public async Task<Project> AddAsync(Project entity)
    {
        await _context.Projects.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Project entity)
    {
        _context.Projects.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Project entity)
    {
        var projectId = entity.ProjectID;

        var flat = await _context.Tasks
            .Where(t => t.ProjectID == projectId)
            .Select(t => new { t.TaskID, t.ParentID })
            .ToListAsync();

        var flatList = flat.Select(x => (x.TaskID, x.ParentID)).ToList();
        var taskIds = flatList.Select(x => x.TaskID).ToHashSet();

        var postOrder = new List<int>();
        foreach (var rootId in flatList
                     .Where(x => x.ParentID is null || !taskIds.Contains(x.ParentID.Value))
                     .Select(x => x.TaskID))
        {
            CollectPostOrderIds(rootId, flatList, postOrder);
        }

        foreach (var (id, _) in flatList)
        {
            if (!postOrder.Contains(id))
                postOrder.Add(id);
        }

        var idSet = postOrder.ToHashSet();

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (idSet.Count > 0)
            {
                await _context.TaskDependencies
                    .Where(d => idSet.Contains(d.PredecessorID) || idSet.Contains(d.SuccessorID))
                    .ExecuteDeleteAsync();

                foreach (var id in postOrder)
                {
                    var task = await _context.Tasks.FindAsync(id);
                    if (task is not null)
                        _context.Tasks.Remove(task);
                }
            }

            _context.Projects.Remove(entity);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    private static void CollectPostOrderIds(int rootId, IReadOnlyList<(int TaskID, int? ParentID)> all, List<int> output)
    {
        foreach (var (childId, _) in all.Where(x => x.ParentID == rootId))
            CollectPostOrderIds(childId, all, output);

        output.Add(rootId);
    }

    public async Task<IEnumerable<Project>> GetByPhraseAsync(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return Array.Empty<Project>();

        var term = s.Trim().ToLowerInvariant();
        return await _context.Projects
            .Where(p =>
                (p.Name ?? string.Empty).ToLower().Contains(term)
                || (p.Description ?? string.Empty).ToLower().Contains(term))
            .ToListAsync();
    }
}
