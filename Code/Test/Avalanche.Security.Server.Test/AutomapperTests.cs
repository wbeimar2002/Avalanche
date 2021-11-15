using System.Linq;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Entities;
using Avalanche.Security.Server.Persistence;
using Avalanche.Security.Server.ViewModels;
using Xunit;
using static Avalanche.Security.Server.Test.Utilities;

namespace Avalanche.Security.Server.Test
{
#pragma warning disable CA1707 // Identifiers should not contain underscores
    public class AutomapperTests
    {
        [Fact]
        public void AutoMapper_MapUserViewModel_ValuesAreEqual()
        {
            // Arrange
            var model = Fakers.GetUserFaker().Generate();
            var mapper = GetMapper(typeof(SecurityDbContext));

            // Act
            var entity = mapper.Map<UserEntity>(model); // Map to Entity
            var mappedModel = mapper.Map<UserViewModel>(entity); // Then Map Entity back to Model

            // Assert
            _ = Assert.IsType<UserViewModel>(model);
            _ = Assert.IsType<UserViewModel>(mappedModel);

            AssertUserMappedCorrectly(model, entity);
        }

        [Fact]
        public void AutoMapper_MapCreateUserViewModel_ValuesAreEqual()
        {
            // Arrange
            var model = Fakers.GetCreateUserFaker().Generate();
            var mapper = GetMapper(typeof(SecurityDbContext));

            // Act
            var entity = mapper.Map<UserEntity>(model); // Map to Entity

            // Assert
            _ = Assert.IsType<CreateUserViewModel>(model);

            AssertCreateUserMappedCorrectly(model, entity);
        }

        [Fact]
        public void AutoMapper_MapAccessTokenViewModel_ValuesAreEqual()
        {
            // Arrange
            var model = Fakers.GetAccessTokenFaker().Generate();
            var mapper = GetMapper(typeof(SecurityDbContext));

            // Act
            var viewModel = mapper.Map<AccessTokenViewModel>(model);

            // Assert
            Assert.Equal(model.Token, viewModel.AccessToken);
            Assert.Equal(model.RefreshToken.Token, viewModel.RefreshToken);
        }

        [Fact]
        public void AutoMapper_MapUserFilterViewModel_ValuesAreEqual()
        {
            // Arrange
            var model = Fakers.GetUserFilterViewModelFaker().Generate();
            var mapper = GetMapper(typeof(SecurityDbContext));

            // Act
            var viewModel = mapper.Map<UserFilterModel>(model);

            // Assert
            Assert.Equal(model.IsDescending, viewModel.IsDescending);
            Assert.Equal(model.Page, viewModel.Page);
            Assert.Equal(model.PageSize, viewModel.PageSize);
            Assert.Equal(model.SearchTerms.Count(), viewModel.SearchTerms.Count());
            Assert.Equal(model.UserSortingColumn, viewModel.UserSortingColumn);
        }

        [Fact]
        public void AutoMapper_MapUserCredentialsViewModel_ValuesAreEqual()
        {
            // Arrange
            var model = Fakers.GetUserCredentialsViewModelFaker().Generate();
            var mapper = GetMapper(typeof(SecurityDbContext));

            // Act
            var viewModel = mapper.Map<UserEntity>(model);

            // Assert
            Assert.Equal(model.LoginName, viewModel.LoginName);
            Assert.Equal(model.Password, viewModel.Password);
        }


        private static void AssertUserMappedCorrectly(UserViewModel model, UserEntity entity)
        {
            Assert.Equal(model.FirstName, entity.FirstName);
            Assert.Equal(model.LastName, entity.LastName);
            Assert.Equal(model.LoginName, entity.LoginName);
        }

        private static void AssertCreateUserMappedCorrectly(CreateUserViewModel model, UserEntity entity)
        {
            Assert.Equal(model.FirstName, entity.FirstName);
            Assert.Equal(model.LastName, entity.LastName);
            Assert.Equal(model.LoginName, entity.LoginName);
        }
    }

#pragma warning restore CA1707 // Identifiers should not contain underscores
}
