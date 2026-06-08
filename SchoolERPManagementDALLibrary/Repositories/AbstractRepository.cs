using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Contexts;
using Microsoft.EntityFrameworkCore;

namespace SchoolERPManagementDALLibrary.Repositories
{
    public class AbstractRepository<K, T> : IRepository<K, T> where T : class
    {
        protected readonly SchoolERPDbContext _context;

        public AbstractRepository(SchoolERPDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(T entity, bool save = true, CancellationToken ct = default)
        {
            await _context.Set<T>().AddAsync(entity, ct);
            if (save)
            {
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task DeleteAsync(T entity, bool save = true, CancellationToken ct = default)
        {
            _context.Set<T>().Remove(entity);
            if (save)
            {
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<T?> GetByIdAsync(params object[] keyValues)
        {
            if (keyValues == null || keyValues.Length == 0) return null;
            var found = await _context.Set<T>().FindAsync(keyValues);
            return found as T;
        }

        public IQueryable<T> Query(bool asNoTracking = false)
        {
            var q = _context.Set<T>().AsQueryable();
            return asNoTracking ? q.AsNoTracking() : q;
        }

        public async Task<IEnumerable<T>> ListAsync(CancellationToken ct = default)
        {
            return await _context.Set<T>().ToListAsync(ct);
        }

        public async Task UpdateAsync(T entity, bool save = true, CancellationToken ct = default)
        {
            _context.Set<T>().Update(entity);
            if (save)
            {
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await _context.SaveChangesAsync(ct);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>>[]? includes = null, int? skip = null, int? take = null, bool asNoTracking = false, CancellationToken ct = default)
        {
            IQueryable<T> query = _context.Set<T>();
            if (includes != null)
            {
                foreach (var inc in includes)
                {
                    query = query.Include(inc);
                }
            }

            if (asNoTracking) query = query.AsNoTracking();

            if (predicate != null) query = query.Where(predicate);

            if (skip.HasValue) query = query.Skip(skip.Value);
            if (take.HasValue) query = query.Take(take.Value);

            return await query.ToListAsync(ct);
        }
    }
}