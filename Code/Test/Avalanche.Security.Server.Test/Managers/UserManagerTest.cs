using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Interfaces;
using Avalanche.Security.Server.Core.Managers;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Shared.Infrastructure.Security.Hashing;
using Moq;
using Xunit;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Avalanche.Security.Server.Test.Managers
{
#pragma warning disable CA1707 // Identifiers should not contain underscores
    public class UsersManagerTest
    {
        [Fact]
        public async Task UserManager_AddUser_Succeeds()
        {
            // Arrange
            var repoMoq = new Mock<IUserRepository>();
            var hashMoq = new Mock<IPasswordHasher>();
            _ = repoMoq.Setup(x => x.AddUser(It.IsAny<NewUserModel>())).Returns(Task.FromResult(Fakers.GetUserFaker().Generate()));
            var usersManager = new UsersManager(repoMoq.Object, hashMoq.Object);

            var toAdd = Fakers.GetNewUserFaker().Generate();

            // Act
            _ = await usersManager.AddUser(toAdd).ConfigureAwait(false);

            // Assert
            repoMoq.Verify(x => x.AddUser(toAdd), Times.Once());
        }

        [Fact]
        public async Task UserManager_DeleteUser_Succeeds()
        {
            // Arrange
            const int idToDelete = 156;
            var repoMoq = new Mock<IUserRepository>();
            var hashMoq = new Mock<IPasswordHasher>();
            _ = repoMoq.Setup(x => x.DeleteUser(idToDelete)).Returns(Task.FromResult(idToDelete));
            var usersManager = new UsersManager(repoMoq.Object, hashMoq.Object);

            // Act
            _ = await usersManager.DeleteUser(idToDelete).ConfigureAwait(false);

            // Assert
            repoMoq.Verify(x => x.DeleteUser(idToDelete), Times.Once());
        }

        [Fact]
        public async Task UserManager_GetAllUsers_Succeeds()
        {
            // Arrange
            var repoMoq = new Mock<IUserRepository>();
            var hashMoq = new Mock<IPasswordHasher>();
            _ = repoMoq.Setup(x => x.GetUsers()).Returns(Task.FromResult(Fakers.GetUserFaker().Generate(10).AsEnumerable()));
            var usersManager = new UsersManager(repoMoq.Object, hashMoq.Object);

            // Act
            _ = await usersManager.GetUsers().ConfigureAwait(false);

            // Assert
            repoMoq.Verify(x => x.GetUsers(), Times.Once());
        }

        [Fact]
        public async Task UserManager_GetUser_Succeeds()
        {
            // Arrange
            var user = Fakers.GetUserFaker().Generate();
            var repoMoq = new Mock<IUserRepository>();
            var hashMoq = new Mock<IPasswordHasher>();
            _ = repoMoq.Setup(x => x.GetUser(It.IsAny<string>())).Returns(Task.FromResult(user)!);
            var usersManager = new UsersManager(repoMoq.Object, hashMoq.Object);

            // Act
            _ = await usersManager.GetUser(user.UserName).ConfigureAwait(false);

            // Assert
            repoMoq.Verify(x => x.GetUser(user.UserName), Times.Once());
        }

        [Fact]
        public async Task UserManager_SearchUsers_Succeeds()
        {
            // Arrange
            var user = Fakers.GetUserFaker().Generate();
            var repoMoq = new Mock<IUserRepository>();
            var hashMoq = new Mock<IPasswordHasher>();
            _ = repoMoq.Setup(x => x.SearchUsers(It.IsAny<string>())).Returns(Task.FromResult(new List<UserModel>() { user }.AsEnumerable()));
            var usersManager = new UsersManager(repoMoq.Object, hashMoq.Object);

            // Act
            _ = await usersManager.SearchUsers(user.UserName).ConfigureAwait(false);

            // Assert
            repoMoq.Verify(x => x.SearchUsers(user.UserName), Times.Once());
        }

        [Fact]
        public async Task UserManager_UpdateUser_Succeeds()

        {
            // Arrange
            var update = Fakers.GetUpdateUserFaker().Generate();
            var repoMoq = new Mock<IUserRepository>();
            var hashMoq = new Mock<IPasswordHasher>();
            _ = repoMoq.Setup(x => x.UpdateUser(update)).Returns(Task.CompletedTask);
            var usersManager = new UsersManager(repoMoq.Object, hashMoq.Object);

            // Act
            await usersManager.UpdateUser(update).ConfigureAwait(false);

            // Assert
            repoMoq.Verify(x => x.UpdateUser(update), Times.Once());
        }

        [Fact]
        public async Task UserManager_UpdateUserPassword_Succeeds()

        {
            // Arrange
            var update = Fakers.GetUpdateUserPasswordFaker().Generate();
            var repoMoq = new Mock<IUserRepository>();
            var hashMoq = new Mock<IPasswordHasher>();
            _ = repoMoq.Setup(x => x.UpdateUserPassword(update)).Returns(Task.CompletedTask);
            var usersManager = new UsersManager(repoMoq.Object, hashMoq.Object);

            // Act
            await usersManager.UpdateUserPassword(update).ConfigureAwait(false);

            // Assert
            repoMoq.Verify(x => x.UpdateUserPassword(update), Times.Once());
        }

        [Fact]
        public async Task UserManager_VerifyUserLogin_LoginFails_WrongPassword()
        {
            // Arrange
            const string providedPassword = "UserProvidedPassword";
            var user = Fakers.GetUserFaker().Generate();
            var repoMoq = new Mock<IUserRepository>();
            var hashMoq = new Mock<IPasswordHasher>();
            _ = repoMoq.Setup(x => x.GetUser(user.UserName)).Returns(Task.FromResult(user)!);

            _ = hashMoq.Setup(x => x.PasswordMatches(providedPassword, user.PasswordHash)).Returns(false);
            var usersManager = new UsersManager(repoMoq.Object, hashMoq.Object);

            // Act
            var (loginValid, userResult) = await usersManager.VerifyUserLogin(user.UserName, providedPassword).ConfigureAwait(false);

            // Assert
            hashMoq.Verify(x => x.PasswordMatches(providedPassword, user.PasswordHash), Times.Once());
            Assert.False(loginValid);
            Assert.Null(userResult);
        }

        [Fact]
        public async Task UserManager_VerifyUuserPassword_LoginSucceeds()
        {
            // Arrange
            const string providedPassword = "UserProvidedPassword";
            var user = Fakers.GetUserFaker().Generate();
            var repoMoq = new Mock<IUserRepository>();
            var hashMoq = new Mock<IPasswordHasher>();
            _ = repoMoq.Setup(x => x.GetUser(user.UserName)).Returns(Task.FromResult(user)!);

            _ = hashMoq.Setup(x => x.PasswordMatches(providedPassword, user.PasswordHash)).Returns(true);
            var usersManager = new UsersManager(repoMoq.Object, hashMoq.Object);

            // Act
            var (loginValid, userResult) = await usersManager.VerifyUserLogin(user.UserName, providedPassword).ConfigureAwait(false);

            // Assert
            hashMoq.Verify(x => x.PasswordMatches(providedPassword, user.PasswordHash), Times.Once());
            Assert.True(loginValid);
            Assert.NotNull(userResult);
            Assert.Equal(user, userResult);
        }

        [Fact]
        public async Task UserManager_VerifyUserLogin_LoginFails_NoUser()
        {
            // Arrange
            const string providedPassword = "UserProvidedPassword";
            var user = Fakers.GetUserFaker().Generate();
            var repoMoq = new Mock<IUserRepository>();
            var hashMoq = new Mock<IPasswordHasher>();
            _ = repoMoq.Setup(x => x.GetUser(user.UserName)).Returns(Task.FromResult((UserModel?)null)!);

            var usersManager = new UsersManager(repoMoq.Object, hashMoq.Object);

            // Act
            var (loginValid, userResult) = await usersManager.VerifyUserLogin(user.UserName, providedPassword).ConfigureAwait(false);

            // Assert
            hashMoq.Verify(x => x.PasswordMatches(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            Assert.False(loginValid);
            Assert.Null(userResult);
        }
#pragma warning restore CA1707 // Identifiers should not contain underscores
    }
}
