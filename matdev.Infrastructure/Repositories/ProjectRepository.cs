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

    private static IQueryable<Project> ProjectsWithLookups(ApplicationDbContext ctx) =>
        ctx.Projects
            .AsNoTracking()
            .Include(p => p.Topic)
            .Include(p => p.Workpackage)
            .Include(p => p.IssueType)
            .Include(p => p.ProjectStatus)
            .Include(p => p.Priority)
            .Include(p => p.Responsible)
            .Include(p => p.Support);

    public async Task<Project?> GetByIdAsync(int id)
    {
        return await ProjectsWithLookups(_context).FirstOrDefaultAsync(p => p.ProjectID == id);
    }

    public async Task<Project?> GetByIdForUpdateAsync(int id)
    {
        return await _context.Projects.FirstOrDefaultAsync(p => p.ProjectID == id);
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        return await ProjectsWithLookups(_context).ToListAsync();
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
        _context.Projects.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Project>> GetByPhraseAsync(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return Array.Empty<Project>();

        var term = s.Trim().ToLowerInvariant();
        return await ProjectsWithLookups(_context)
            .Where(p =>
                (p.Name ?? string.Empty).ToLower().Contains(term)
                || (p.Description ?? string.Empty).ToLower().Contains(term))
            .ToListAsync();
    }
}
