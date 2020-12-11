using Microsoft.AspNetCore.SignalR;

namespace Avalanche.Api.Hubs
{
    // NOTE: SignalR Hubs are transient. Do not embed any stateful logic. 
    //      - The hub should contain only any necessary client connection lifetime handling logic (via OnConnectedAsync/OnDisconnectedAsync overrides) and any exposed endpoints.
    //      - so in the (likely) event that there are no exposed endpoints and no specific logic around client connections, this should remain essentially empty.
    public class BroadcastHub : Hub<IBroadcastHubClient>
    {
        public const string BroadcastHubRoute = "/broadcast";

        public BroadcastHub()
        {
        }
    }
}
