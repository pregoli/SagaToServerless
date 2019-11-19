using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;

namespace SagaToServerless.Business
{
    public abstract class EntityBase
    {
        [BsonId(IdGenerator = typeof(GuidGenerator)), BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
    }
}
