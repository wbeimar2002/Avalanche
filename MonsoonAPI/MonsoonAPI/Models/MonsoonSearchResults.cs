using ISM.LibrarySi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonsoonAPI.Models
{
    public class MonsoonSearchResults
    {
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int ItemCount { get; set; }
        public List<Dictionary<ESearchFields, string>> Result { get; set; }

        public MonsoonSearchResults(int totalItems, int currentPage, int itemCount, List<Dictionary<ESearchFields, string>> results)
        {
            TotalItems = totalItems;
            CurrentPage = currentPage;
            ItemCount = itemCount;
            Result = results;
        }
    }
}
