using System.Linq;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Avalanche.Security.Server.Persistence
{
    public class UserRepository : IUserRepository
    {
        private readonly SecurityDbContext _context;

        public UserRepository(SecurityDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User user, ERole[] userRoles)
        {
            var roleNames = userRoles.Select(r => r.ToString()).ToList();
            var roles = await _context.Roles.Where(r => roleNames.Contains(r.Name)).ToListAsync();

            foreach(var role in roles)
            {
                user.UserRoles.Add(new UserRole { RoleId = role.Id });
            }
               
            _context.Users.Add(user);
        }

        public async Task<User> FindByEmailAsync(string email)
        {
            return await _context.Users.Include(u => u.UserRoles)
                                       .ThenInclude(ur => ur.Role)
                                       .SingleOrDefaultAsync(u => u.Email == email);
        }
    }
}