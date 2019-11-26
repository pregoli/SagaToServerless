using SagaToServerless.Business;
using SagaToServerless.Common.Models;
using SagaToServerless.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SagaToServerless.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Guid> SaveAsync(string createdBy, UserModel user, List<Guid> groupIds)
        {
            //throw new Exception("BOOOM");
            var userCreated = await _userRepository.SaveAsync(new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                GroupIds = groupIds.Select(x => x.ToString()).ToList(),
                CreatedBy = createdBy
            });

            return userCreated.Id;
        }

        public async Task<bool> UnassignGroupsFromUser(Guid userId, List<Guid> groupIds)
        {
            return await _userRepository.UnassignGroupsFromUser(userId, groupIds);
        }
    }
}
