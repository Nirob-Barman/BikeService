using System.Linq.Expressions;
using BikeService.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BikeService.Infrastructure.Persistence
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }


        public async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<TResult?> GetByIdAsync<TResult>(object id, Expression<Func<T, TResult>> selector)
        {
            return await _dbSet
                .Where(entity => EF.Property<object>(entity, "Id") == id) // Ensure you're querying by the id.
                .Select(selector)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<TResult>> GetAllAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            return await _dbSet.Select(selector).ToListAsync();
        }

        public async Task<IEnumerable<TResult>> GetAllAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector)
        {
            return await _dbSet.Where(predicate).Select(selector).ToListAsync();
        }

        public async Task<IEnumerable<TResult>> GetAllWithIncludesAsync<TResult>(Expression<Func<T, TResult>> selector,
            params Expression<Func<T, object>>[] includes)
        {
            var query = _dbSet.AsQueryable();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.Select(selector).ToListAsync();
        }

        public async Task<IEnumerable<TResult>> GetAllWithIncludesAsync<TResult>(Expression<Func<T, bool>> predicate,
            Expression<Func<T, TResult>> selector,
            params Expression<Func<T, object>>[] includes)
        {
            var query = _dbSet.AsQueryable();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.Where(predicate).Select(selector).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            return await _dbSet.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<IEnumerable<TResult>> GetPagedAsync<TResult>(int pageNumber, int pageSize, Expression<Func<T, TResult>> selector)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            return await _dbSet
                .Select(selector)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<TResult>> GetPagedAsync<TResult>(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            return await _dbSet.Where(predicate).Select(selector).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<IEnumerable<TResult>> GetPagedAsync<TResult>(int pageNumber, int pageSize, Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> searchExpression = null!)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var allRecords = await _dbSet.ToListAsync();

            if (searchExpression != null)
            {
                allRecords = allRecords.Where(searchExpression.Compile()).ToList();
            }

            var projectedRecords = allRecords.Select(selector.Compile()).ToList();

            var pagedRecords = projectedRecords.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return pagedRecords;
        }

        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<int> CountAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector)
        {
            return await _dbSet.Where(predicate).Select(selector).CountAsync();
        }

        public async Task<IEnumerable<T>> Where(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<TResult>> Where<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector)
        {
            return await _dbSet.Where(predicate).Select(selector).ToListAsync();
        }

        public async Task<IEnumerable<TResult>> GetDistinctAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            return await _dbSet.Select(selector).Distinct().ToListAsync();
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector)
        {
            return await _dbSet.Where(predicate).Select(selector).FirstOrDefaultAsync();
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.AsQueryable();

            foreach (var include in includes)
                query = query.Include(include);

            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }














        //--------------------------------------------------------













        public IQueryable<T> GetAllAsQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public IQueryable<TResult> GetAllAsQueryable<TResult>(Expression<Func<T, TResult>> selector)
        {
            return _dbSet.Select(selector);  // returns IQueryable, allowing further chaining of methods
        }

        public IQueryable<TResult> GetAllAsQueryable<TResult>(Expression<Func<T, TResult>> selector,
            string? searchTerm = null,
            Expression<Func<T, bool>>? filter = null,
            string sortBy = "Id",
            bool sortDesc = false,
            int pageNumber = 1,
            int pageSize = 10)
        {
            IQueryable<T> query = _dbSet;

            // Apply the filter predicate if provided
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Apply search term filter if provided (for example, search in a "Name" field)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                // You can extend this part to search on specific fields as needed
                query = query.Where(e => EF.Functions.Like(EF.Property<string>(e, "Name"), $"%{searchTerm}%"));
            }

            // Apply sorting
            if (sortDesc)
            {
                query = sortBy switch
                {
                    "Name" => query.OrderByDescending(e => EF.Property<object>(e, "Name")),
                    "Id" => query.OrderByDescending(e => EF.Property<object>(e, "Id")),
                    _ => query.OrderByDescending(e => EF.Property<object>(e, sortBy)) // Default sort by field
                };
            }
            else
            {
                query = sortBy switch
                {
                    "Name" => query.OrderBy(e => EF.Property<object>(e, "Name")),
                    "Id" => query.OrderBy(e => EF.Property<object>(e, "Id")),
                    _ => query.OrderBy(e => EF.Property<object>(e, sortBy)) // Default sort by field
                };
            }

            // Apply pagination
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            // Project to the desired result type
            return query.Select(selector);
        }

        public IQueryable<T> PaginateAsQueryable(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1;
            if (pageSize < 1)
                pageSize = 10;

            return _dbSet
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);  // returns IQueryable, allowing further chaining of methods
        }

        public async Task<IEnumerable<TResult>> PaginateAsQueryable<TResult>(IQueryable<T> query, int pageNumber, int pageSize, Expression<Func<T, TResult>> selector)
        {
            if (pageNumber < 1)
                pageNumber = 1;
            if (pageSize < 1)
                pageSize = 10;

            return await query
                .Select(selector)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public IQueryable<T> WithIncludesAsQueryable(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return query;
        }


        // Filter records based on provided conditions
        public IQueryable<T> Where(IQueryable<T> query, Expression<Func<T, bool>> predicate)
        {
            return query.Where(predicate);
        }
        public IQueryable<TResult> WhereAsQueryable<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector)
        {
            return _dbSet.Where(predicate).Select(selector);  // returns IQueryable, allowing further chaining of methods
        }

        public IQueryable<T> WhereIn(IQueryable<T> query, Expression<Func<T, string>> field, IEnumerable<string> values)
        {
            return query.Where(e => values.Contains(EF.Property<string>(e, field.Body.ToString())));
        }

        // Sort records
        public IQueryable<T> OrderBy(IQueryable<T> query, Expression<Func<T, object>> keySelector, bool ascending = true)
        {
            return ascending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
        }

        public IQueryable<T> OrderBy(IQueryable<T> query, string sortBy, string sortOrder = "ASC", List<string> validSortColumns = null!)
        {
            // Default to "Name" if no validSortColumns are passed and if sortBy is not in the valid list
            //validSortColumns ??= new List<string> { "Id", "Name" }; // Add other columns as needed

            //if (!validSortColumns.Contains(sortBy))
            //{
            //    sortBy = "Id";
            //}

            sortOrder = sortOrder?.ToUpper() == "DESC" ? "DESC" : "ASC"; // Default to "ASC" if invalid

            // Build the dynamic sorting expression using reflection
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, sortBy);
            var lambda = Expression.Lambda<Func<T, object>>(Expression.Convert(property, typeof(object)), parameter);

            return sortOrder == "DESC" ? query.OrderByDescending(lambda) : query.OrderBy(lambda);
        }



        // Apply pagination
        public IQueryable<T> PaginateAsQueryable(IQueryable<T> query, int pageNumber, int pageSize)
        {
            return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        // Select specific fields (projection)
        public IQueryable<TResult> Select<TResult>(IQueryable<T> query, Expression<Func<T, TResult>> selector)
        {
            return query.Select(selector);
        }

        public async Task<List<T>> ToListAsync(IQueryable<T> query)
        {
            return await query.ToListAsync();
        }

        public async Task<List<TResult>> ToListAsync<TResult>(IQueryable<T> query, Expression<Func<T, TResult>> selector)
        {
            // Apply the projection and execute the query asynchronously to return the results as a List<TResult>
            return await query.Select(selector).ToListAsync();
        }

        public async Task<IEnumerable<TResult>> ToEnumerableAsync<TResult>(IQueryable<T> query, Expression<Func<T, TResult>> selector)
        {
            //This method is similar to ToListAsync,
            //but it returns IEnumerable<TResult>.
            //The use of ToListAsync() at the end makes it return a list,
            //which is actually List < TResult >, not IEnumerable<TResult>.

            // Apply the projection and execute the query asynchronously to return the results as a IEnumerable<TResult>
            return await query.Select(selector).ToListAsync();


            //If the intention is to return an IEnumerable<TResult>
            //(which is a more general interface than List<TResult>),
            //then the method name should reflect that.
            //However, the use of ToListAsync() will force the return type to List<TResult>,
            //so either change the return type to List<TResult>(matching the ToListAsync behavior)
            //or change the method to return IEnumerable<TResult> with an appropriate call
            //(e.g., ToListAsync() can be replaced with AsEnumerable() if you truly want IEnumerable).

            //var result = await query.Select(selector).ToListAsync(); // still fetches the data
            //return result.AsEnumerable(); // Returns IEnumerable<TResult>
        }

        public bool Any(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Any(predicate);
        }

        public async Task<int> CountAsync(IQueryable<T> query)
        {
            return await query.CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> searchExpression = null!)
        {
            var query = _dbSet.AsQueryable();

            if (searchExpression != null)
            {
                query = query.Where(searchExpression);
            }

            return await query.CountAsync();
        }


        public IQueryable<TResult> DistinctByAsQueryable<TResult>(Expression<Func<T, TResult>> selector)
        {
            return _dbSet.Select(selector).Distinct();  // returns IQueryable, allowing further chaining of methods
        }

        public async Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            return await _dbSet.MaxAsync(selector);
        }

        public async Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            return await _dbSet.MinAsync(selector);
        }

        public IQueryable<TResult> FirstOrDefaultAsQueryable<TResult>(Expression<Func<T, bool>> predicate,
            Expression<Func<T, TResult>> selector)
        {
            return _dbSet
                .Where(predicate)
                .Select(selector)
                .Take(1);  // returns IQueryable, allowing further chaining of methods
        }

        public IQueryable<TResult> GroupByAsQueryable<TResult>(Expression<Func<T, object>> groupByKey,
            Expression<Func<IGrouping<object, T>, TResult>> selector)
        {
            return _dbSet.GroupBy(groupByKey).Select(selector);
        }


        //Works only for entity types
        public IQueryable<TResult> ExecuteRawSql<TResult>(string sqlQuery, params object[] parameters)
        {
            return (IQueryable<TResult>)_dbSet.FromSqlRaw(sqlQuery, parameters).AsQueryable();
        }





        //--------------------------------------------------------




        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }
    }
}
