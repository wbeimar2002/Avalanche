using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Repositories;

namespace Avalanche.Security.Server.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SecurityDbContext _context;

        public UnitOfWork(SecurityDbContext context)
        {
            _context = context;
        }

        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}