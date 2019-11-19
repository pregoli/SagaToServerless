using MongoDB.Driver;
using SagaToServerless.Business;
using SagaToServerless.Common.Enums;
using System;
using System.Threading.Tasks;

namespace SagaToServerless.Data.Repositories
{
    public class GroupRepository : Repository<Group>, IGroupRepository
    {
        public GroupRepository(string connectionString, string collectionName)
            : base(connectionString, collectionName)
        {
        }

        public async Task<Guid> AssignUserAsync(Guid groupId, string userId)
        {
            var filter = Builders<Group>.Filter.Eq(x => x.Id, groupId);
            var update = Builders<Group>.Update.AddToSet(u => u.Users, userId);

            var result =  await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });

            return result.MatchedCount == 1 ? groupId : Guid.Empty;
        }

        public async Task UpdateAsync(Group group)
        {
            var filter = Builders<Group>.Filter.Eq(x => x.Id, group.Id);
            var builder = Builders<Group>.Update;
            var update = builder
                .Set(u => u.Id, group.Id)
                .Set(u => u.GroupName, group.GroupName)
                .Set(u => u.Users, group.Users);

            await _collection.UpdateOneAsync(filter, update);
        }
    }
}
