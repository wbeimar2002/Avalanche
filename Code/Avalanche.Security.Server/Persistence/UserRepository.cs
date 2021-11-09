using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Core.Repositories;
using Ism.Utility.Core;
using Microsoft.EntityFrameworkCore;
using static Avalanche.Security.Server.Extensions.DynamicSortingExtensions;

namespace Avalanche.Security.Server.Persistence
{
    public class UserRepository : IUserRepository
    {
        public const int MaxPageSize = 100;
        public const int MinSearchTermLength = 2;
        private readonly SecurityDbContext _context;

        public UserRepository(SecurityDbContext context) => _context = context;

        public async Task AddAsync(User user, ERole[] userRoles)
        {
            var roleNames = userRoles.Select(r => r.ToString()).ToList();
            var roles = await _context.Roles.Where(r => roleNames.Contains(r.Name)).ToListAsync().ConfigureAwait(false);

            foreach (var role in roles)
            {
                user.UserRoles.Add(new UserRole { RoleId = role.Id });
            }

            _context.Users.Add(user);

            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<User> FindByLoginAsync(string loginName) => await _context.Users.Include(u => u.UserRoles)
                                       .ThenInclude(ur => ur.Role)
                                       .SingleOrDefaultAsync(u => u.LoginName == loginName).ConfigureAwait(false);

        public async Task<List<User>> GetUsers(UserFilterModel filter)
        {
            var validatedSearchTerms = ValidateSearchTerms(filter.SearchTerms);
            var searchExpression = validatedSearchTerms.Any() ? FormatAsMatchExpression(validatedSearchTerms) : "<None>";

            var baseFtsQuery = _context.Users;
            IQueryable<User> userQuery;

            if (validatedSearchTerms.Any())
            {
                // TODO search with filter logic here
                // Define query for FTS keyword search
                //userQuery = baseFtsQuery
                //    .Where(x => x.Match == searchExpression)
                //    .Select(x => x.User);
                userQuery = _context.Users;
            }
            else
            {
                // Otherwise just query DmagIndexes directly
                userQuery = _context.Users;
            }

            return await userQuery
               .OrderBy(filter.UserSortingColumn.ToString(), filter.IsDescending)
               .Skip(filter.Page * filter.PageSize)
               .Take(filter.PageSize)
               .ToListAsync()
               .ConfigureAwait(false);
        }

        private static string FormatAsMatchExpression(IEnumerable<string> searchTerms) =>
           // Takes a enumerable of strings i.e. ["one", "two"]
           // And formats them as a Sqlite FTS5 MATCH expression, i.e. '"one"* "two*"'
           // https://www.sqlite.org/fts5.html
           searchTerms
               .Select(searchTerm => searchTerm.Replace("\"", "\"\"", StringComparison.Ordinal)) // In Sqlite FTS MATCH, double quotes are escaped by adding a second double quote.  i.e. " => ""
               .Select(searchTerm => $"\"{searchTerm}\"*") // Then the whole string is placed in double quotes and a * is placed outside the quoted string to indicate a "starts with" search expression
               .JoinStrings(" "); //Then all the "starts with" expressions can be joined, separated by a space to create the final search expression


        private static IEnumerable<string> ValidateSearchTerms(IEnumerable<string> searchTerms) =>
            searchTerms
                .Where(x => !string.IsNullOrWhiteSpace(x) && x.Length >= MinSearchTermLength);
    }
}
