using Ism.Broadcaster.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Ism.Broadcaster.Services
{
    public class BroadcasterClientService : IBroadcasterClientService
    {
        private readonly ILogger _logger;

        public BroadcasterClientService(
            ILogger<BroadcasterClientService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> Send(Uri endpoint, MessageRequest message)
        {
            try
            {
                string token = string.Empty;
                using (var client = new HttpClient())
                {
                    string bodyRequest = JsonConvert.SerializeObject(message);

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    var response = await client.PostAsync(endpoint, new StringContent(bodyRequest, System.Text.Encoding.UTF8, "application/json"));

                    if (!response.IsSuccessStatusCode)
                    {
                        string data = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Error sending notification to the broadcaster. Response {data}");
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending notification to the boadcaster", ex);
                return false;
            }
        }
    }
}
