using SagaToServerless.Business;
using SagaToServerless.Common.Models;
using SagaToServerless.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SagaToServerless.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;
        public GroupService(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
        }

        public async Task<Guid> SaveAsync(string createdBy, GroupModel groupModel)
        {
            var group = await _groupRepository.SaveAsync(new Group
            {
                GroupName = groupModel.GroupName,
                CreatedBy = createdBy,
                Users = groupModel.Users
            });

            return group.Id;
        }

        public async Task<Guid> AssignUserAsync(Guid groupId, string userId)
        {
            return await _groupRepository.AssignUserAsync(groupId, userId);
        }

        public async Task<bool> RemoveAsync(Guid id)
        {
            return await _groupRepository.RemoveAsync(id);
        }
    }
}
