using System.Threading.Tasks;

namespace Avalanche.Security.Server.Test.Repositories
{
#pragma warning disable CA1707 // Identifiers should not contain underscores
    public interface IUserRepositoryTests
    {
        Task AddUser_Duplicate_Throws();
        Task AddUser_FirstNameEmpty_ThrowsValidationException();

        Task AddUser_FirstNameNull_ThrowsValidationException();

        Task AddUser_FirstNameTooLong_ThrowsValidationException();

        Task AddUser_LastNameEmpty_ThrowsValidationException();

        Task AddUser_LastNameNull_ThrowsValidationException();

        Task AddUser_LastNameTooLong_ThrowsValidationException();

        Task AddUser_MultithreadedWrites_Succeeds();

        Task AddUser_PasswordEmpty_ThrowsValidationException();

        Task AddUser_PasswordNull_ThrowsValidationException();

        Task AddUser_UnexpectedError_LogsExceptionAndThrows();

        Task AddUser_UserNameEmpty_ThrowsValidationException();

        Task AddUser_UserNameNull_ThrowsValidationException();

        Task AddUser_UserNameTooLong_ThrowsValidationException();

        Task AddUser_WriteSucceeds();

        Task DeleteUser_DefaultUserId_Throws();

        Task DeleteUser_DeleteSucceeds();

        Task DeleteUser_LogsExceptionAndThrows();

        Task GetAllUsers_ReadSucceeds();

        Task GetUser_MultithreadedReadsSucceed();

        Task GetUsers_ReadSucceeds();

        Task UpdateUser_FirstNameEmpty_ThrowsValidationException();

        Task UpdateUser_FirstNameNull_ThrowsValidationException();

        Task UpdateUser_FirstNameTooLong_ThrowsValidationException();

        Task UpdateUser_LastNameEmpty_ThrowsValidationException();

        Task UpdateUser_LastNameNull_ThrowsValidationException();

        Task UpdateUser_LastNameTooLong_ThrowsValidationException();

        Task UpdateUser_MultithreadedUpdates_Succeeds();
        Task UpdateUser_Succeeds();

        // Task UpdateUser_DuplicateUserName_Throws();

        Task UpdateUser_UnexpectedError_LogsExceptionAndThrows();

        Task UpdateUser_UserNameEmpty_ThrowsValidationException();

        Task UpdateUser_UserNameNull_ThrowsValidationException();

        Task UpdateUser_UserNameTooLong_ThrowsValidationException();

        Task UpdateUser_UserNotExist_Throws();

        Task UpdateUser_UserNull_Throws();

        Task UpdateUserPassword_PasswordEmpty_ThrowsValidationException();

        Task UpdateUserPassword_PasswordNull_ThrowsValidationException();

        Task UpdateUserPassword_Succeeds();
        Task UpdateUserPassword_UnexpectedError_LogsExceptionAndThrows();
    }
#pragma warning restore CA1707 // Identifiers should not contain underscores
}
