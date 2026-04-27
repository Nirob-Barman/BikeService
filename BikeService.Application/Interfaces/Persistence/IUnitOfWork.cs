namespace BikeService.Application.Interfaces.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> Repository<T>() where T : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task BeginTransaction();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
