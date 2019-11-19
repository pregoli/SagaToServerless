using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SagaToServerless.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Business
{
    [BsonIgnoreExtraElements]
    public class Group : EntityBase
    {
        public Group()
        {
            Users = new List<string>();
        }

        public string GroupName { get; set; }
        public string CreatedBy { get; set; }
        
        [BsonRepresentation(BsonType.String)]
        public GroupType GroupType { get; set; }
        public List<string> Users { get; set; }
    }
}
