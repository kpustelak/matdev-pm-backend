using matdev.Domain.Entities.TaskEntities;
using matdev.Domain.Interfaces;
using matdev.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace matdev.Infrastructure.Repositories;

public class TaskCategoryRepository : ITaskCategoryRepository
{
    private readonly ApplicationDbContext _context;

    public TaskCategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaskCategory?> GetByIdAsync(int id)
    {
        return await _context.TaskCategories.FindAsync(id);
    }

    public async Task<IEnumerable<TaskCategory>> GetAllAsync()
    {
        return await Task.FromResult(_context.TaskCategories.ToList());
    }

    public async Task<TaskCategory> AddAsync(TaskCategory entity)
    {
        await _context.TaskCategories.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(TaskCategory entity)
    {
        _context.TaskCategories.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TaskCategory entity)
    {
        _context.TaskCategories.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TaskCategory>> GetByPhraseAsync(string phrase)
    {
        if (string.IsNullOrWhiteSpace(phrase))
            return Array.Empty<TaskCategory>();

        var term = phrase.Trim().ToLowerInvariant();
        return await _context.TaskCategories
            .Where(tc => (tc.Name ?? string.Empty).ToLower().Contains(term))
            .ToListAsync();
    }
}
