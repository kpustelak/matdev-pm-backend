using matdev.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace matdev.Domain.Interfaces
{
    public interface IRepository<T>
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<IEnumerable<T>> GetByPhraseAsync(string s);
    }
}
