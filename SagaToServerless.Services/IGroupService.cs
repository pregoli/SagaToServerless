using SagaToServerless.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SagaToServerless.Services
{
    public interface IGroupService
    {
        Task<Guid> SaveAsync(string createdBy, GroupModel groupModel);
        Task<bool> RemoveAsync(Guid id);
        Task<Guid> AssignUserAsync(Guid groupId, string userId);
    }
}
