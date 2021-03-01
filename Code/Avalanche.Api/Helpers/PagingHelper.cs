using Avalanche.Api.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Helpers
{
    public static class PagingHelper
    {
        public static void AppendPagingContext<TFilterViewModel, TResult>(IUrlHelper Url, HttpRequest Request, TFilterViewModel filter, PagedCollectionViewModel<TResult> result)
            where TFilterViewModel : FilterViewModelBase
            where TResult : class
        {
            //TODO: Not sure the UI is consuming this at the moment.  May need to revisit paging mechanism later depending on UI implementation?

            //Get next page URL string  
            TFilterViewModel nextFilter = filter.Clone() as TFilterViewModel;
            nextFilter.Page += 1;
            String nextUrl = result.Items.Count() <= 0 ? null : Url.Action("Get", null, nextFilter, Request.Scheme);

            //Get previous page URL string  
            TFilterViewModel previousFilter = filter.Clone() as TFilterViewModel;
            previousFilter.Page -= 1;
            String previousUrl = previousFilter.Page <= 0 ? null : Url.Action("Get", null, previousFilter, Request.Scheme);

            result.NextPage = !String.IsNullOrWhiteSpace(nextUrl) ? new Uri(nextUrl) : null;
            result.PreviousPage = !String.IsNullOrWhiteSpace(previousUrl) ? new Uri(previousUrl) : null;
        }
    }
}
