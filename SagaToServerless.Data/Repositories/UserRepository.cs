using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using SagaToServerless.Business;

namespace SagaToServerless.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(string connectionString, string collectionName)
            : base(connectionString, collectionName)
        {
        }

        public async Task<bool> UnassignGroupsFromUser(Guid userId, List<Guid> groupIds)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, userId);
            var update = Builders<User>.Update.PullAll(u => u.GroupIds, groupIds.Select(x => x.ToString()));

            var result = await _collection.UpdateOneAsync(filter, update);

            return result.MatchedCount == 1;
        }
    }
}
