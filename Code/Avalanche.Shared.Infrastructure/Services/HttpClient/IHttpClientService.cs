using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Shared.Infrastructure.Services.HttpClient
{
    public interface IHttpClientService
    {
        Task<HttpResponseMessage> PostWithBearerAuthAsync<T>(Uri serviceUrl, string token, T request);
    }
}
