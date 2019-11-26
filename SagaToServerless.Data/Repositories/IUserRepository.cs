using SagaToServerless.Business;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SagaToServerless.Data.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> UnassignGroupsFromUser(Guid userId, List<Guid> groupIds);
    }
}
