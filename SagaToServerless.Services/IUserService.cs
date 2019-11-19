using SagaToServerless.Common.Models;
using System;
using System.Threading.Tasks;

namespace SagaToServerless.Services
{
    public interface IUserService
    {
        Task<Guid> SaveAsync(string createdBy, UserModel user);
        Task<bool> RemoveAsync(Guid id);
    }
}
