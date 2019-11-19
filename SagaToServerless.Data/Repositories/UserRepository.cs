using SagaToServerless.Business;

namespace SagaToServerless.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(string connectionString, string collectionName)
            : base(connectionString, collectionName)
        {
        }
    }
}
