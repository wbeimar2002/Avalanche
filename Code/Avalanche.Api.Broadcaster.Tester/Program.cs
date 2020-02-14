using Avalanche.Api.Broadcaster.Enumerations;
using Avalanche.Api.Broadcaster.EventArgs;
using Avalanche.Api.Broadcaster.Services;
using System;

namespace Avalanche.Api.Broadcaster.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            string hubURL = "http://localhost:32772/broadcaster";
            EventNameEnum hubEventName = EventNameEnum.Unknown;

            var listener = new BroadcastListenerService(hubURL, hubEventName);
            listener.ListenHubEvent(EventListened);

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }

        private static void EventListened(object suscriber, BroadcastEventArgs args)
        {
            Console.WriteLine($"New message: {args.MessageRequest.Message}");
        }
    }
}
