using DatabaseContext;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Repositories;

public class Repository<T>: IRepository<T> where T : class
{
	private readonly AI_ColdCall_Agent_DbContext _context;

	public Repository(AI_ColdCall_Agent_DbContext context)
	{
		_context = context;
	}

	//Get one Item
	public async Task<T> GetByIdAsync(int id)
	{
		return await _context.Set<T>().FindAsync(id);
	}
	public T FindOneItem(Expression<Func<T, bool>> creiteria, string[]? includes = null)
	{
		IQueryable<T> query = _context.Set<T>();

		if (includes != null)
		{
			foreach (var include in includes)
			{
				query = query.Include(include);
			}
		}
		return query.SingleOrDefault(creiteria);
	}
	public T FindOneItemWithInclude(string[]? includes = null)
	{
		IQueryable<T> query = _context.Set<T>();

		if (includes != null)
		{
			foreach (var include in includes)
			{
				query = query.Include(include);
			}
		}
		return query.SingleOrDefault();
	}


	//Get list of Items
	public async Task<IEnumerable<T>> GetAllAsync()
	{
		return await _context.Set<T>().ToListAsync();
	}
	public async Task<IEnumerable<T>> FindAllWithIncludeAsync(string[]? includes = null)
	{
		IQueryable<T> queries = _context.Set<T>();

		if (includes != null)
		{
			foreach (var include in includes)
			{
				queries = queries.Include(include);
			}
		}
		return await queries.ToListAsync();
	}
	public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, string[]? includes = null)
	{
		IQueryable<T> queries = _context.Set<T>();

		if (includes != null)
		{
			foreach (var include in includes)
			{
				queries = queries.Include(include);
			}
		}
		return await queries.Where(criteria).ToListAsync();
	}

	//Add Item
	public async Task<T> AddAsync(T entity)
	{
		await _context.AddAsync(entity);
		return entity;
	}

	//Delete Item
	public void Delete(T entity)
	{
		_context.Remove(entity);
	}

	//Update Item
	public T Update(T entity)
	{
		_context.Update(entity);
		return entity;
	}

	public async Task<int> Count(Expression<Func<T, bool>> criteria)
	{
		return await _context.Set<T>().CountAsync(criteria);
	}

	public async Task<double> Sum(Expression<Func<T, double>> criteria)
	{
		return await _context.Set<T>().SumAsync(criteria);
	}
}
