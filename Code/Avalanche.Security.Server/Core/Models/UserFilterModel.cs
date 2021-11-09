using System.Collections.Generic;
using System.Linq;

namespace Avalanche.Security.Server.Core.Models
{
    public class UserFilterModel
    {
        public IEnumerable<string> SearchTerms { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public UserSortingColumn UserSortingColumn { get; set; }
        public bool IsDescending { get; set; }

        public UserFilterModel() { }

        public UserFilterModel(IEnumerable<string>? searchTerms = null, int page = 0, int pageSize = 10,
            UserSortingColumn userSortingColumn = UserSortingColumn.LastName, bool isDescending = false)
        {
            SearchTerms = searchTerms ?? Enumerable.Empty<string>();
            Page = page;
            PageSize = pageSize;
            UserSortingColumn = userSortingColumn;
            IsDescending = isDescending;
        }
    }
}
