using matdev.Domain.Entities;
using matdev.Domain.Interfaces;
using matdev.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace matdev.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<IEnumerable<User>> GetByPhraseAsync(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return Array.Empty<User>();

        var term = s.Trim().ToLowerInvariant();
        return await _context.Users
            .Where(x =>
                (x.FirstName ?? string.Empty).ToLower().Contains(term)
                || (x.LastName ?? string.Empty).ToLower().Contains(term)
                || (x.Email ?? string.Empty).ToLower().Contains(term))
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User> AddAsync(User user)
    {
        var newUser = await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return newUser.Entity;
    }

    public async Task UpdateAsync(User user)
    {
        var userToEdit = await _context.Users.FindAsync(user.UserID);
        if (userToEdit is null)
        {
            throw new KeyNotFoundException("No user found");
        }
        
        userToEdit.Email = user.Email;
        userToEdit.FirstName = user.FirstName;
        userToEdit.LastName = user.LastName;
        userToEdit.PhoneNumber = user.PhoneNumber;

        _context.Users.Update(userToEdit);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}