using MongoDB.Driver;
using SagaToServerless.Business;
using System;
using System.Threading.Tasks;

namespace SagaToServerless.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : EntityBase
    {
        protected IMongoCollection<T> _collection;

        public Repository(string connectionString, string collection)
        {
            var mongoUrl = new MongoUrl(connectionString);
            var settings = MongoClientSettings.FromUrl(mongoUrl);

            var mongoClient = new MongoClient(settings);
            var database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
            _collection = database.GetCollection<T>(collection);
        }

        public async Task<T> SaveAsync(T entity)
        {
            if (entity.Id == Guid.Empty)
                await _collection.InsertOneAsync(entity);
            else
                await ReplaceOneAsync(entity.Id, entity);

            return entity;
        }

        public async Task<T> GetAsync(Guid id)
        {
            var filter = Builders<T>.Filter.Eq("Id", id);
            return await _collection.Find<T>(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> RemoveAsync(Guid id)
        {
            var filter = Builders<T>.Filter.Eq("Id", id);
            var result = await _collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<T> ReplaceOneAsync(Guid id, T entity)
        {
            var filter = Builders<T>.Filter.Eq("Id", id);

            await _collection.ReplaceOneAsync(filter, entity,
            new UpdateOptions { IsUpsert = true });

            return entity;
        }
    }
}
