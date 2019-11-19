using SagaToServerless.Business;
using System;
using System.Threading.Tasks;

namespace SagaToServerless.Data.Repositories
{
    public interface IGroupRepository : IRepository<Group>
    {
        Task<Guid> AssignUserAsync(Guid groupId, string userId);
        Task UpdateAsync(Group group);
    }
}
