using Ism.Broadcaster.Enumerations;
using Ism.Broadcaster.EventArgs;
using Ism.Broadcaster.Services;
using System;

namespace Ism.Broadcaster.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            string hubURL = "https://localhost:5001/broadcast";
            EventGroupEnum hubEventName = EventGroupEnum.OnTesting;

            var listener = new BroadcastListenerService(hubURL, hubEventName);
            listener.ListenHubEvent(EventListened);

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }

        private static void EventListened(object suscriber, BroadcastEventArgs args)
        {
            Console.WriteLine($"New message: {args.MessageRequest.Content}");
        }
    }
}
