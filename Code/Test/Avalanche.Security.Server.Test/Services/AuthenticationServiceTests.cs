using System;
using System.Threading.Tasks;
using Avalanche.Api.Core.Security.Tokens;
using Avalanche.Api.Core.Services;
using Avalanche.Api.Managers;
using Avalanche.Security.Server.Core.Interfaces;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Core.Security.Hashing;
using Moq;
using Xunit;

namespace Avalanche.Security.Tests.Services
{
    public class AuthenticationServiceTests
    {
        private bool _calledRefreshToken = false;

        private Mock<IUsersService> _userRepository;
        private Mock<IPasswordHasher> _passwordHasher;
        private Mock<ITokenHandler> _tokenHandler;

        private IAuthenticationManager _authenticationService;

        public AuthenticationServiceTests()
        {
            SetupMocks();
            //Temporary: Working in refactor
            _authenticationService = new Api.Managers.AuthenticationManager(_userRepository.Object, _passwordHasher.Object, _tokenHandler.Object);
        }

        private void SetupMocks()
        {
            _userRepository = new Mock<IUserRepository>();

            //_userService.Setup(u => u.FindByEmailAsync("invalid@invalid.com"))
            //           .Returns(Task.FromResult<UserModel>(null));

            //_userService.Setup(u => u.FindByEmailAsync("test@test.com"))
            //            .ReturnsAsync(new User
            //            {
            //                Id = 1,
            //                Email = "test@test.com",
            //                Password = "123",
            //                UserRoles = new Collection<UserRole>
            //                {
            //                    new UserRole
            //                    {
            //                        UserId = 1,
            //                        RoleId = 1,
            //                        Role = new Role
            //                        {
            //                            Id = 1,
            //                            Name = ERole.Common.ToString()
            //                        }
            //                    }
            //                }
            //            });

            _passwordHasher = new Mock<IPasswordHasher>();
            _passwordHasher.Setup(ph => ph.PasswordMatches(It.IsAny<string>(), It.IsAny<string>()))
                           .Returns<string, string>((password, hash) => password == hash);

            _tokenHandler = new Mock<ITokenHandler>();
            _tokenHandler.Setup(h => h.CreateAccessToken(It.IsAny<UserModel>()))
                         .Returns(new AccessToken
                                     (
                                        token: "abc",
                                        expiration: DateTime.UtcNow.AddSeconds(30).Ticks,
                                        refreshToken: new RefreshToken
                                                          (
                                                              token: "abc",
                                                              expiration: DateTime.UtcNow.AddSeconds(60).Ticks
                                                          )
                                     )
                                 );

            _tokenHandler.Setup(h => h.TakeRefreshToken("abc"))
                         .Returns(new RefreshToken("abc", DateTime.UtcNow.AddSeconds(60).Ticks));

            _tokenHandler.Setup(h => h.TakeRefreshToken("expired"))
                         .Returns(new RefreshToken("expired", DateTime.UtcNow.AddSeconds(-60).Ticks));

            _tokenHandler.Setup(h => h.TakeRefreshToken("invalid"))
                         .Returns<RefreshToken>(null);

            _tokenHandler.Setup(h => h.RevokeRefreshToken("abc"))
                         .Callback(() => _calledRefreshToken = true);
        }

        [Fact]
        public async Task Should_Create_Access_Token_For_Valid_Credentials()
        {
            var response = await _authenticationService.CreateAccessTokenAsync("test@test.com", "123");

            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.NotNull(response.Token);
            Assert.NotNull(response.Token.RefreshToken);
            Assert.Equal("abc", response.Token.Token);
            Assert.Equal("abc", response.Token.RefreshToken.Token);
            Assert.False(response.Token.IsExpired());
            Assert.False(response.Token.RefreshToken.IsExpired());
        }

        [Fact]
        public async Task Should_Not_Create_Access_Token_For_Non_Existing_User()
        {
            var response = await _authenticationService.CreateAccessTokenAsync("invalid@invalid.com", "123");

            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("Invalid credentials.", response.Message);
        }

        [Fact]
        public async Task Should_Not_Create_Access_Token_For_Invalid_Password()
        {
            var response = await _authenticationService.CreateAccessTokenAsync("invalid@invalid.com", "321");

            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("Invalid credentials.", response.Message);
        }

        [Fact]
        public async Task Should_Refresh_Token_For_Valid_Refresh_Token()
        {
            var response = await _authenticationService.RefreshTokenAsync("abc", "test@test.com");

            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal("abc", response.Token.Token);
        }

        [Fact]
        public async Task Should_Not_Refresh_Token_When_Token_Is_Expired()
        {
            var response = await _authenticationService.RefreshTokenAsync("expired", "test@test.com");

            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("Expired refresh token.", response.Message);
        }

        [Fact]
        public async Task Should_Not_Refresh_Token_For_Invalid_User_Data()
        {
            var response = await _authenticationService.RefreshTokenAsync("invalid", "test@test.com");

            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("Invalid refresh token.", response.Message);
        }

        [Fact]
        public void Should_Revoke_Refresh_Token()
        {
            _authenticationService.RevokeRefreshToken("abc");
            
            Assert.True(_calledRefreshToken);
        }
    }
}
