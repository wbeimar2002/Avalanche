using System.Threading.Tasks;

namespace Avalanche.Security.Server.Test
{
    public interface IUserRepositoryTests
    {
#pragma warning disable CA1707 // Identifiers should not contain underscores
        Task Repository_AddUser_Succeeds();
        Task Repository_FindUser_Succeeds();
        Task Repository_FindUser_Fails();
        Task Repository_AddUser_PasswordMissing_Throws();
        Task Repository_AddUser_LoginNameMissing_Throws();
#pragma warning restore CA1707 // Identifiers should not contain underscores
    }
}
