using System.Collections.Generic;
using System.Linq;
using Avalanche.Security.Server.Core.Models;

namespace Avalanche.Security.Server.ViewModels
{
    public class UserFilterViewModel
    {
        public IEnumerable<string>? SearchTerms { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; } = 10;
        public UserSortingColumn UserSortingColumn { get; set; } = UserSortingColumn.LastName;
        public bool IsDescending { get; set; }

        public UserFilterViewModel() { }

        public UserFilterViewModel(IEnumerable<string>? searchTerms = null, int page = 0, int pageSize = 10,
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
