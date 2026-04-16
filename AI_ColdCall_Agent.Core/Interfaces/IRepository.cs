using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces;

public interface IRepository<T> where T : class
{
	Task<T> GetByIdAsync(int id);
	T FindOneItem(Expression<Func<T, bool>> creiteria, string[]? includes = null);
	T FindOneItemWithInclude(string[]? includes = null);

	Task<IEnumerable<T>> GetAllAsync(string includeProperties = null);
	Task<IEnumerable<T>> FindAllWithIncludeAsync(string[]? includes = null);
	Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, string[]? includes = null);
	Task<IEnumerable<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes);

	Task<(IEnumerable<T> Items, int TotalCount)> GetPaginatedAsync(
		Expression<Func<T, bool>> filter,
		string[] includes,
		Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
		int pageNumber,
		int pageSize);

	Task<T> AddAsync(T entity);
	T Update(T entity);
	void Delete(T entity);

	Task<int> Count(Expression<Func<T, bool>> criteria);
	Task<double> Sum(Expression<Func<T, double>> criteria);
	Task<T> GetFirstOrDefaultWithStringsAsync(Expression<Func<T, bool>> criteria, string[] includes = null);
}
