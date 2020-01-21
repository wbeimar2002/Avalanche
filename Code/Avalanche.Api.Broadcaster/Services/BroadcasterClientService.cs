using Avalanche.Api.Broadcaster.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Services.HttpClient;
using Avalanche.Shared.Infrastructure.Services.Logger;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Api.Broadcaster.Services
{
    public class BroadcasterClientService : IBroadcasterClientService
    {
        private readonly IAppLoggerService _logger;
        private readonly IHttpClientService _httpClientService;

        public BroadcasterClientService(
            IAppLoggerService logger,
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
                    _logger.Log(LogType.Error, $"Error sending notification to the cloud boadcaster. Response {data}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Log(LogType.Error, $"Error sending notification to the cloud boadcaster", ex);
                return false;
            }
        }
    }
}
