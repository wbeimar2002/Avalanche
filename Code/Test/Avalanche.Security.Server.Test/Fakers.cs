using System;
using System.Collections.Generic;
using Avalanche.Security.Server.Core.Security.Tokens;
using Avalanche.Security.Server.ViewModels;
using Bogus;

namespace Avalanche.Security.Server.Test
{
    public static class Fakers
    {
        private static readonly Random Random = new Random();

        public static Faker<UserViewModel> GetUserFaker() =>
           new Faker<UserViewModel>()
               .CustomInstantiator(f =>
               new UserViewModel
               {
                   FirstName = f.Random.AlphaNumeric(RandomInt(f, 10, 50)),
                   LastName = f.Random.AlphaNumeric(RandomInt(f, 10, 50)),
                   LoginName = f.Random.AlphaNumeric(RandomInt(f, 10, 50))
               });

        public static Faker<CreateUserViewModel> GetCreateUserFaker() =>
           new Faker<CreateUserViewModel>()
               .CustomInstantiator(f =>
               new CreateUserViewModel
               {
                   FirstName = f.Random.AlphaNumeric(RandomInt(f, 10, 50)),
                   LastName = f.Random.AlphaNumeric(RandomInt(f, 10, 50)),
                   LoginName = f.Random.AlphaNumeric(RandomInt(f, 10, 50))
               });

        public static Faker<AccessToken> GetAccessTokenFaker() =>
          new Faker<AccessToken>()
              .CustomInstantiator(f =>
              new AccessToken(f.Random.AlphaNumeric(RandomInt(f, 10, 50)), 10, new RefreshToken(f.Random.AlphaNumeric(RandomInt(f, 10, 50)), 10)));

        public static Faker<UserFilterViewModel> GetUserFilterViewModelFaker() =>
           new Faker<UserFilterViewModel>()
               .CustomInstantiator(f =>
               new UserFilterViewModel
               {
                   IsDescending = RandomBool(),
                   Page = RandomInt(f),
                   UserSortingColumn = RandomBool() ? Core.Models.UserSortingColumn.LastName : Core.Models.UserSortingColumn.LoginName,
                   PageSize = RandomInt(f),
                   SearchTerms = new List<string>() { f.Random.AlphaNumeric(RandomInt(f, 10, 50)) }
               });

        public static Faker<UserCredentialsViewModel> GetUserCredentialsViewModelFaker() =>
           new Faker<UserCredentialsViewModel>()
               .CustomInstantiator(f =>
               new UserCredentialsViewModel
               {
                   LoginName = f.Random.AlphaNumeric(RandomInt(f, 10, 50)),
                   Password = f.Random.AlphaNumeric(RandomInt(f, 10, 50))
               });

        private static bool RandomBool() =>
           Random.NextDouble() == 1; // By default NextDouble() returns 0 or 1
        private static int RandomInt(Faker f, int lowerBoundary = 20, int upperBoundary = 100) => f.Random.Int(lowerBoundary, upperBoundary);
    }
}
