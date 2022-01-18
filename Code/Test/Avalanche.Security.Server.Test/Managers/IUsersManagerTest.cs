using System.Threading.Tasks;

namespace Avalanche.Security.Server.Test.Managers
{
    public interface IUsersManagerTest
    {
        Task AddUserManagerTest();
        Task AddUser_MultithreadedWritesSucceed();
        Task AddUser_NameNull_ThrowsValidationException();
        Task AddUser_NameTooLong_ThrowsValidationException();
        Task AddUser_UnexpectedError_LogsExceptionAndThrows();
        Task DeleteUser_DeleteSucceeds();
        Task DeleteUser_LogsExceptionAndThrows();
        Task GetUsers_ReadSucceeds();
        Task GetUser_MultithreadedReadsSucceed();
        Task Repository_AddUser_NameEmpty_ThrowsValidationException();
        Task Repository_GetAllUsers_ReadSucceeds();
        Task UpdateUser_Success();
        Task UpdateUser_WhenUserNameIsNull();
        Task UpdateUser_When_UserIsNull();
        Task UpdateUser_When_UserNotExist();
    }
}