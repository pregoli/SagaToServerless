using SagaToServerless.Common.Enums;
using System;
using System.Collections.Generic;

namespace SagaToServerless.Common.Models
{
    public class GroupModel
    {
        public GroupModel()
        {
            Users = new List<string>();
        }

        public Guid Id { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public GroupType GroupType { get; set; }
        public List<string> Users { get; set; }
    }
}
