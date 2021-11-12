using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Core.Repositories;
using Avalanche.Security.Server.Core.Security.Hashing;
using Avalanche.Security.Server.Core.Services;
using Avalanche.Security.Server.Entities;
using Avalanche.Security.Server.Services;
using Moq;
using Xunit;

namespace Avalanche.Security.Tests.Services
{
    public class UserServiceTests
    {
        private Mock<IPasswordHasher> _passwordHasher;
        private Mock<IUserRepository> _userRepository;
        private Mock<IUnitOfWork> _unitOfWork;

        private readonly IUserService _userService;

        public UserServiceTests()
        {
            SetupMocks();
            _userService = new UserService(_userRepository.Object, _unitOfWork.Object, _passwordHasher.Object);
        }

        private void SetupMocks()
        {
            _passwordHasher = new Mock<IPasswordHasher>();
            _passwordHasher.Setup(ph => ph.HashPassword(It.IsAny<string>())).Returns("123");

            _userRepository = new Mock<IUserRepository>();
            _userRepository.Setup(r => r.FindByLoginAsync("test@test.com"))
                .ReturnsAsync(new UserEntity { Id = 1, LoginName = "test@test.com", UserRoles = new Collection<UserRole>() });

            _userRepository.Setup(r => r.FindByLoginAsync("secondtest@secondtest.com"))
                .Returns(Task.FromResult<UserEntity>(null));

            _userRepository.Setup(r => r.AddAsync(It.IsAny<UserEntity>(), It.IsAny<ERole[]>())).Returns(Task.CompletedTask);

            _unitOfWork = new Mock<IUnitOfWork>();
            _unitOfWork.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Should_Create_Non_Existing_User()
        {
            var user = new UserEntity { LoginName = "mytestuser@mytestuser.com", Password = "123", UserRoles = new Collection<UserRole>() };

            var response = await _userService.CreateUserAsync(user, ERole.Common).ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal(user.LoginName, response.User.LoginName);
            Assert.Equal(user.Password, response.User.Password);
        }

        [Fact]
        public async Task Should_Not_Create_User_When_LoginName_Is_Alreary_In_Use()
        {
            var user = new UserEntity { LoginName = "test@test.com", Password = "123", UserRoles = new Collection<UserRole>() };

            var response = await _userService.CreateUserAsync(user, ERole.Common).ConfigureAwait(false);

            Assert.False(response.Success);
            Assert.Equal("Login name already in use.", response.Message);
        }

        [Fact]
        public async Task Should_Find_Existing_User_By_LoginName()
        {
            var user = await _userService.FindByLoginAsync("test@test.com").ConfigureAwait(false);
            Assert.NotNull(user);
            Assert.Equal("test@test.com", user.LoginName);
        }

        [Fact]
        public async Task Should_Return_Null_When_Trying_To_Find_User_By_Invalid_LoginName()
        {
            var user = await _userService.FindByLoginAsync("secondtest@secondtest.com").ConfigureAwait(false);
            Assert.Null(user);
        }
    }
}
