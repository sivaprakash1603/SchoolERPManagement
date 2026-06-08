using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementDALLibrary.Interfaces;

public interface IRepository<K, T> where T : class
{
    // Basic list/query access
    IQueryable<T> Query(bool asNoTracking = false);
    Task<IEnumerable<T>> ListAsync(CancellationToken ct = default);

    // Flexible finders
    Task<T?> GetByIdAsync(params object[] keyValues);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>>[]? includes = null, int? skip = null, int? take = null, bool asNoTracking = false, CancellationToken ct = default);

    // Mutations with optional auto-save
    Task AddAsync(T entity, bool save = true, CancellationToken ct = default);
    Task UpdateAsync(T entity, bool save = true, CancellationToken ct = default);
    Task DeleteAsync(T entity, bool save = true, CancellationToken ct = default);

    // Unit-of-work control
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}