using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SagaToServerless.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SagaToServerless.Business
{
    public class User : EntityBase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string CreatedBy { get; set; }
    }
}
