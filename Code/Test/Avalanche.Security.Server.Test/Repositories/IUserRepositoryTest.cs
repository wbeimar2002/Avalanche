using System.Threading.Tasks;
using Xunit;

namespace Avalanche.Security.Server.Test.Repositories
{
    public interface IUserRepositoryTest
    {       
        Task AddUser_Duplicate_Throws();
        Task AddUser_MultithreadedWritesSucceed();
        Task AddUser_NameNull_ThrowsValidationException();
        Task AddUser_NameTooLong_ThrowsValidationException();
        Task AddUser_UnexpectedError_LogsExceptionAndThrows();
        Task AddUser_WriteSucceeds();
        Task DeleteUser_DeleteSucceeds();
        Task DeleteUser_LogsExceptionAndThrows();
        Task GetUsers_ReadSucceeds();
        Task GetUser_MultithreadedReadsSucceed();
        Task Repository_AddUser_NameEmpty_ThrowsValidationException();
        Task Repository_GetAllUsers_ReadSucceeds();
        Task UpdateUser_When_UserIsNull();
        Task UpdateUser_When_UserNotExist();
    }
}
