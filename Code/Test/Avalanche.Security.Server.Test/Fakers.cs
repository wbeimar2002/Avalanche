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
                    Id = f.Person.Random.Number(100000, 999999),
                    FirstName = f.Person.FirstName,
                    LastName = f.Person.LastName,
                    UserName = f.Person.UserName,
                    PasswordHash = f.Random.AlphaNumeric(64)
                });

        public static Faker<NewUserModel> GetNewUserFaker() =>
            new Faker<NewUserModel>()
            .CustomInstantiator(f =>
                new NewUserModel()
                {
                    FirstName = f.Person.FirstName,
                    LastName = f.Person.LastName,
                    UserName = f.Person.UserName,
                    Password = f.Random.AlphaNumeric(64)
                });

        public static Faker<UpdateUserModel> GetUpdateUserFaker() =>
            new Faker<UpdateUserModel>()
            .CustomInstantiator(f =>
                new UpdateUserModel()
                {
                    FirstName = f.Person.FirstName,
                    LastName = f.Person.LastName,
                    UserName = f.Person.UserName
                });

        public static Faker<UpdateUserPasswordModel> GetUpdateUserPasswordFaker() =>
            new Faker<UpdateUserPasswordModel>()
            .CustomInstantiator(f =>
                new UpdateUserPasswordModel()
                {
                    UserName = f.Person.UserName,
                    Password = f.Random.AlphaNumeric(64)
                });
    }
}
