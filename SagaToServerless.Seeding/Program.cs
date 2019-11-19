using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SagaToServerless.Business;
using SagaToServerless.Common;
using SagaToServerless.Common.Enums;
using SagaToServerless.Data.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SagaToServerless.Seeding
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Seed();
        }

        private static async Task Seed()
        {
            var groupRepository = new GroupRepository("mongodb://localhost:27017/SagaToServerless?maxPoolSize=150", Constants.CollectionNames.Groups);

            var groupTypes = new List<GroupType> { GroupType.Office365, GroupType.Distribution, GroupType.Security, GroupType.Teams };
            foreach(var groupType in groupTypes)
            {
                for (int i = 0; i < 3; i++)
                {
                    var group = new Group
                    {
                        GroupName = $"Group_{groupType.ToString()}_{i}",
                        GroupType = groupType,
                        CreatedBy = "paolo.regoli@coreview.com"
                    };

                    await groupRepository.SaveAsync(group);
                }
            }
        }
    }
}
