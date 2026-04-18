using matdev.Domain.Entities.LookupEntities;
using matdev.Domain.Interfaces;
using matdev.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace matdev.Infrastructure.Repositories
{
    public class WorkpackageRepository : IWorkpackageRepository
    {
        private readonly ApplicationDbContext _context;
        public WorkpackageRepository(ApplicationDbContext context)
        {
            _context = context; 
        }
        public async Task<Workpackage?> GetByIdAsync(int id)
        {
            return await _context.Workpackages.FindAsync(id);
        }
        public async Task<IEnumerable<Workpackage>> GetAllAsync()
        {
            return await Task.FromResult(_context.Workpackages.ToList());
        }
        public async Task<Workpackage> AddAsync(Workpackage entity)
        {
            await _context.Workpackages.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task UpdateAsync(Workpackage entity)
        {
            _context.Workpackages.Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Workpackage entity)
        {
            _context.Workpackages.Remove(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Workpackage>> GetByPhraseAsync(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                return Array.Empty<Workpackage>();

            var term = phrase.Trim().ToLowerInvariant();
            return await _context.Workpackages
                .Where(wp =>
                    (wp.Name ?? string.Empty).ToLower().Contains(term))
                .ToListAsync();
        }
    }
}
