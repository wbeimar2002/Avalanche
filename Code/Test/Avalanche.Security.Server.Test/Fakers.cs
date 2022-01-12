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
                    Id = f.Person.Random.Number(10000, 99999),
                    FirstName = f.Person.FirstName,
                    LastName = f.Person.LastName,
                    UserName = f.Person.UserName,
                    Password = f.Random.AlphaNumeric(64)
                });
    }
}
