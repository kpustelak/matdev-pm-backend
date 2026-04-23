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
        _context.Projects.Remove(entity);
        await _context.SaveChangesAsync();
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
