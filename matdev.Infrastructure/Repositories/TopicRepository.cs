using matdev.Domain.Entities.LookupEntities;
using matdev.Domain.Interfaces;
using matdev.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace matdev.Infrastructure.Repositories
{
    public class TopicRepository : ITopicRepository
    {
        private readonly ApplicationDbContext _context;
        public TopicRepository(ApplicationDbContext context)
        {
            _context = context; 
        }
        public async Task<Topic?> GetByIdAsync(int id)
        {
            return await _context.Topics.FindAsync(id);
        }
        public async Task<IEnumerable<Topic>> GetAllAsync()
        {
            return await Task.FromResult(_context.Topics.ToList());
        }
        public async Task<Topic> AddAsync(Topic entity)
        {
            await _context.Topics.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task UpdateAsync(Topic entity)
        {
            _context.Topics.Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Topic entity)
        {
            _context.Topics.Remove(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Topic>> GetByPhraseAsync(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                return Array.Empty<Topic>();

            var term = phrase.Trim().ToLowerInvariant();
            return await _context.Topics
                .Where(t =>
                    (t.Name ?? string.Empty).ToLower().Contains(term))
                .ToListAsync();
        }
    }
}
