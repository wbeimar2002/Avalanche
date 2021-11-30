using System.Threading.Tasks;

namespace Avalanche.Security.Server.Core.Repositories
{
    public interface IUnitOfWork
    {
        Task CompleteAsync();
    }
}