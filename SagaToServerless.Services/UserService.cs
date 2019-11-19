using SagaToServerless.Business;
using SagaToServerless.Common.Models;
using SagaToServerless.Data.Repositories;
using System;
using System.Threading.Tasks;

namespace SagaToServerless.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Guid> SaveAsync(string createdBy, UserModel user)
        {
            var userCreated = await _userRepository.SaveAsync(new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                CreatedBy = createdBy
            });

            return userCreated.Id;
        }

        public async Task<bool> RemoveAsync(Guid id)
        {
            return await _userRepository.RemoveAsync(id);
        }
    }
}
