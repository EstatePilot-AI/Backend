using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Interfaces;

public interface IRepository<T> where T : class
{
	//Get one Item
	Task<T> GetByIdAsync(int id);
	T FindOneItem(Expression<Func<T, bool>> creiteria, string[]? includes = null);
	T FindOneItemWithInclude(string[]? includes = null);

	//Get list of Items
	Task<IEnumerable<T>> GetAllAsync();
	Task<IEnumerable<T>> FindAllWithIncludeAsync(string[]? includes = null);
	Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, string[]? includes = null);

	//Add Item
	Task<T> AddAsync(T entity);

	//Update Item
	T Update(T entity);

	//Delete Item
	void Delete(T entity);
	
	Task<int> Count(Expression<Func<T, bool>> criteria);
	Task<double> Sum(Expression<Func<T, double>> criteria);
}
