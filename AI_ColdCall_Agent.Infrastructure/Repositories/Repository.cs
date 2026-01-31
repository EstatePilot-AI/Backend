using DatabaseContext;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Repositories
{
    public class Repository<T> : IRepository <T> where T : class
    {
        protected readonly AI_ColdCall_Agent_DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AI_ColdCall_Agent_DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

      

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }



        public async Task<IEnumerable<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }


        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, string[]? includes = null)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.Where(criteria).ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAllWithIncludeAsync(string[]? includes = null)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.ToListAsync();
        }

        public T FindOneItem(Expression<Func<T, bool>> criteria, string[]? includes = null)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return query.SingleOrDefault(criteria);
        }

        public T FindOneItemWithInclude(string[]? includes = null)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return query.SingleOrDefault();
        }

     

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public T Update(T entity)
        {
            _dbSet.Update(entity);
            return entity;
        }

      
        public async Task<int> Count(Expression<Func<T, bool>> criteria)
        {
            return await _dbSet.CountAsync(criteria);
        }

        public async Task<double> Sum(Expression<Func<T, double>> criteria)
        {
            return await _dbSet.SumAsync(criteria);
        }

        public Task<IEnumerable<T>> GetAllAsync(string includeProperties = null)
        {
            throw new NotImplementedException();
        }

        
    }
}
