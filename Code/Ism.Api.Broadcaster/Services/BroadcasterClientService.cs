using Ism.Api.Broadcaster.Models;
using Ism.Api.Broadcaster.Services.HttpClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ism.Api.Broadcaster.Services
{
    public class BroadcasterClientService : IBroadcasterClientService
    {
        private readonly ILogger _logger;
        private readonly IHttpClientService _httpClientService;

        public BroadcasterClientService(
            ILogger<BroadcasterClientService> logger,
            IHttpClientService httpClientService)
        {
            _logger = logger;
            _httpClientService = httpClientService;
        }

        public async Task<bool> Send(Uri endpoint, MessageRequest message)
        {
            try
            {
                //TODO: Pending token auth
                var response = await _httpClientService.PostWithBearerAuthAsync(endpoint, string.Empty, message);

                if (!response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error sending notification to the cloud broadcaster. Response {data}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending notification to the cloud boadcaster", ex);
                return false;
            }
        }
    }
}
