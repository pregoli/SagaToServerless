using SagaToServerless.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SagaToServerless.Services
{
    public interface IUserService
    {
        Task<Guid> SaveAsync(string createdBy, UserModel user, List<Guid> groupIds);
        Task<bool> UnassignGroupsFromUser(Guid userId, List<Guid> groupIds);
    }
}
