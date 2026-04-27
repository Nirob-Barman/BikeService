using System.Linq.Expressions;

namespace BikeService.Application.Interfaces.Persistence
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(object id);
        Task<TResult?> GetByIdAsync<TResult>(object id, Expression<Func<T, TResult>> selector);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<TResult>> GetAllAsync<TResult>(Expression<Func<T, TResult>> selector);
        Task<IEnumerable<TResult>> GetAllAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);
        Task<IEnumerable<TResult>> GetAllWithIncludesAsync<TResult>(Expression<Func<T, TResult>> selector, params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<TResult>> GetAllWithIncludesAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<TResult>> GetPagedAsync<TResult>(int pageNumber, int pageSize, Expression<Func<T, TResult>> selector);
        Task<IEnumerable<TResult>> GetPagedAsync<TResult>(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);
        Task<int> CountAsync();
        Task<int> CountAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);
        Task<IEnumerable<T>> Where(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<TResult>> Where<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);
        Task<IEnumerable<TResult>> GetDistinctAsync<TResult>(Expression<Func<T, TResult>> selector);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<T, bool>> predicate,
            Expression<Func<T, TResult>> selector);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);



        //--------------------------------------------------------


        IQueryable<T> GetAllAsQueryable();
        IQueryable<TResult> GetAllAsQueryable<TResult>(Expression<Func<T, TResult>> selector);
        IQueryable<TResult> GetAllAsQueryable<TResult>(Expression<Func<T, TResult>> selector, string? searchTerm = null, Expression<Func<T, bool>>? filter = null, string sortBy = "Id", bool sortDesc = false, int pageNumber = 1, int pageSize = 10);
        IQueryable<T> PaginateAsQueryable(int pageNumber, int pageSize);
        Task<IEnumerable<TResult>> PaginateAsQueryable<TResult>(IQueryable<T> query, int pageNumber, int pageSize, Expression<Func<T, TResult>> selector);
        IQueryable<T> WithIncludesAsQueryable(params Expression<Func<T, object>>[] includes);
        IQueryable<T> Where(IQueryable<T> query, Expression<Func<T, bool>> predicate);
        IQueryable<TResult> WhereAsQueryable<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);
        IQueryable<T> WhereIn(IQueryable<T> query, Expression<Func<T, string>> field, IEnumerable<string> values);
        IQueryable<T> OrderBy(IQueryable<T> query, Expression<Func<T, object>> keySelector, bool ascending = true);
        IQueryable<T> OrderBy(IQueryable<T> query, string sortBy, string sortOrder = "ASC", List<string> validSortColumns = null!);
        IQueryable<T> PaginateAsQueryable(IQueryable<T> query, int pageNumber, int pageSize);
        IQueryable<TResult> Select<TResult>(IQueryable<T> query, Expression<Func<T, TResult>> selector);
        Task<List<T>> ToListAsync(IQueryable<T> query);
        Task<List<TResult>> ToListAsync<TResult>(IQueryable<T> query, Expression<Func<T, TResult>> selector);
        Task<IEnumerable<TResult>> ToEnumerableAsync<TResult>(IQueryable<T> query, Expression<Func<T, TResult>> selector);
        bool Any(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(IQueryable<T> query);
        Task<int> CountAsync(Expression<Func<T, bool>> searchExpression = null!);
        IQueryable<TResult> DistinctByAsQueryable<TResult>(Expression<Func<T, TResult>> selector);
        Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector);
        Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector);
        IQueryable<TResult> FirstOrDefaultAsQueryable<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);
        IQueryable<TResult> GroupByAsQueryable<TResult>(Expression<Func<T, object>> groupByKey, Expression<Func<IGrouping<object, T>, TResult>> selector);
        IQueryable<TResult> ExecuteRawSql<TResult>(string sqlQuery, params object[] parameters);



        //--------------------------------------------------------



        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}
