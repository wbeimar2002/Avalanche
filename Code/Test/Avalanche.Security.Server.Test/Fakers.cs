using System;
using System.Collections.Generic;
using System.Text;
using Avalanche.Security.Server.Core.Models;
using Bogus;

namespace Avalanche.Security.Server.Test
{
    public static class Fakers
    {
        public static Faker<UserModel> GetUserFaker() =>
                    new Faker<UserModel>()
                        .CustomInstantiator(f =>
                            new UserModel()
                            {
                                Id = f.Random.Number(1000, 9999),
                                FirstName = f.Name.FirstName(Bogus.DataSets.Name.Gender.Male),
                                LastName = f.Name.LastName(Bogus.DataSets.Name.Gender.Female),
                                UserName = f.Name.Random.AlphaNumeric(20),
                                Password = f.Random.AlphaNumeric(64)
                            }
                        );
    }
}
