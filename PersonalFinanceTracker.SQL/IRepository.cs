using PersonalFinanceTracker.Model;

namespace PersonalFinanceTracker.SQL
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(object id);
        Task<List<T>> FindByAsync(RepositoryModel<T> repositoryModel);
        Task<List<TResult>> FindByAsync<TResult>(RepositoryModel<T, TResult> repositoryModel, bool? isEncrypted = false);
        Task<PagedEntities<T>> FindByPageAsync(RepositoryModel<T> repositoryModel);
        Task<T> GetAsync(RepositoryModel<T> repositoryModel);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(object id);
        Task DeleteAsync(T entity);
    }
}
