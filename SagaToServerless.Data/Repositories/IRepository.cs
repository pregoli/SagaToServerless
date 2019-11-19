using SagaToServerless.Business;
using System;
using System.Threading.Tasks;

namespace SagaToServerless.Data.Repositories
{
    public interface IRepository<T> where T : EntityBase
    {
        Task<T> GetAsync(Guid id);

        Task<T> SaveAsync(T entity);

        Task<T> ReplaceOneAsync(Guid id, T entity);

        Task<bool> RemoveAsync(Guid id);
    }
}
