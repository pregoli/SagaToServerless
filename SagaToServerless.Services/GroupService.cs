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
                MemberIds = groupModel.Users
            });

            return group.Id;
        }

        public async Task<Guid> AssignUserAsync(Guid groupId, string userId)
        {
            //throw new Exception("BOOOOM!");
            return await _groupRepository.AssignUserAsync(groupId, userId);
        }
    }
}
