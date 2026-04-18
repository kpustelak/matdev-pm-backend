using matdev.Domain.Entities.LookupEntities;
using matdev.Domain.Interfaces;
using matdev.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace matdev.Infrastructure.Repositories
{
    public class IssueTypeRepository : IIssueTypeRepository
    {
        private readonly ApplicationDbContext _context;
        public IssueTypeRepository(ApplicationDbContext context)
        {
            _context = context; 
        }
        public async Task<IssueType?> GetByIdAsync(int id)
        {
            return await _context.IssueTypes.FindAsync(id);
        }
        public async Task<IEnumerable<IssueType>> GetAllAsync()
        {
            return await Task.FromResult(_context.IssueTypes.ToList());
        }
        public async Task<IssueType> AddAsync(IssueType entity)
        {
            await _context.IssueTypes.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task UpdateAsync(IssueType entity)
        {
            _context.IssueTypes.Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(IssueType entity)
        {
            _context.IssueTypes.Remove(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<IssueType>> GetByPhraseAsync(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                return Array.Empty<IssueType>();

            var term = phrase.Trim().ToLowerInvariant();
            return await _context.IssueTypes
                .Where(it =>
                    (it.Name ?? string.Empty).ToLower().Contains(term))
                .ToListAsync();
        }
    }
}
