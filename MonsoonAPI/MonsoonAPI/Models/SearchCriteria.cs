using System;
using Newtonsoft.Json;

namespace MonsoonAPI.Models {
    public class SearchCriteria {
        [JsonProperty(PropertyName = "login")]
        public string Login { get; private set; }
        [JsonProperty(PropertyName = "keyword")]
        public string Keyword { get; private set; }
        [JsonProperty(PropertyName = "startDt")]
        public DateTimeOffset? StartDt { get; private set; }
        [JsonProperty(PropertyName = "endDt")]
        public DateTimeOffset? EndDt { get; private set; }
        [JsonProperty(PropertyName = "requestedPageNumber")]
        public int RequestedPageNumber { get; private set; }
        [JsonProperty(PropertyName = "numberOfItems")]
        public int NumberOfItems { get; private set; }
        [JsonProperty(PropertyName = "showRawOnly")]
        public bool ShowRawOnly { get; private set; }
        [JsonProperty(PropertyName = "orderBy")]
        public string OrderBy { get; private set; }
        [JsonProperty(PropertyName = "sortAscending")]
        public bool SortAscending { get; private set; }

        public SearchCriteria(string login, string keyword, DateTimeOffset? startDt, DateTimeOffset? endDt, int requestedPageNumber, int numberOfItems, bool showRawOnly, string orderBy, bool sortAscending) {
            Login = login;
            Keyword = keyword;
            StartDt = startDt;
            EndDt = endDt;
            RequestedPageNumber = requestedPageNumber;
            NumberOfItems = numberOfItems;
            ShowRawOnly = showRawOnly;
            OrderBy = orderBy;
            SortAscending = sortAscending;
        }

        public override string ToString() => $"{nameof(Login)}: {Login}, {nameof(Keyword)}: {Keyword}, " +
                                             $"{nameof(StartDt)}: {StartDt}, {nameof(EndDt)}: {EndDt}, " +
                                             $"{nameof(RequestedPageNumber)}: {RequestedPageNumber}, {nameof(NumberOfItems)}: {NumberOfItems}, " +
                                             $"{nameof(ShowRawOnly)}: {ShowRawOnly}, {nameof(OrderBy)}: {OrderBy}, " +
                                             $"{nameof(SortAscending)}: {SortAscending}";
    }
}