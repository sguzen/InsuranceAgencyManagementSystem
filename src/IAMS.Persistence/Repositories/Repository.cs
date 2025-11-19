using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IAMS.Persistence.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.Where(x => !x.IsDeleted && x.Id == id).FirstOrDefaultAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.Where(x => !x.IsDeleted).ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).Where(x => !x.IsDeleted).ToListAsync();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(x => !x.IsDeleted).FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            // Audit timestamps are set by DbContext.SaveChangesAsync
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            // Audit timestamps are set by DbContext.SaveChangesAsync
            var entityList = entities.ToList();
            await _dbSet.AddRangeAsync(entityList);
            return entityList;
        }

        public virtual void Update(T entity)
        {
            // ModifiedOn timestamp is set by DbContext.SaveChangesAsync
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            // ModifiedOn timestamp is set by DbContext.SaveChangesAsync
            foreach (var entity in entities)
            {
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        public virtual void Remove(T entity)
        {
            // Soft delete - ModifiedOn timestamp is set by DbContext.SaveChangesAsync
            entity.IsDeleted = true;
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            // Soft delete - ModifiedOn timestamp is set by DbContext.SaveChangesAsync
            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.Where(x => !x.IsDeleted).CountAsync();

            return await _dbSet.Where(x => !x.IsDeleted).CountAsync(predicate);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(x => !x.IsDeleted).AnyAsync(predicate);
        }
    }
}