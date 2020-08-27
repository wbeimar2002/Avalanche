
using Ism.Broadcaster.EventArgs;
using Ism.Broadcaster.Models;
using Ism.Broadcaster.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;

namespace Ism.Broadcaster.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            string hubURL = "https://localhost:5001/broadcast";
            //Please use the nuget Ism.Broadcaster.Standard
            var listener = new BroadcastListenerService((IHubConfiguration) new HubConfiguration
            {
                HubURL = hubURL,
                HubEventName = "TestingEvent",
                IsHubListeningEnabled = true
            });

            listener.ListenHubEvent(EventListened);

            Console.WriteLine("Waiting for messages or press [enter] to start the testing");
            Console.ReadLine();

            //Sample to send messages
            var logger = serviceProvider.GetService<ILogger<BroadcasterClientService>>();
            var client = new BroadcasterClientService(logger);

            Console.WriteLine("Press [enter] to send the direct test message.");
            Console.ReadLine();

            SendDirectMessage(client);

            Console.WriteLine("Press [enter] to send the queued  test message.");
            Console.ReadLine();

            SendQueuedMessage(client);

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }

        private static async void SendDirectMessage(BroadcasterClientService client)
        {
            var url = "https://localhost:5001/Notifications/direct";
            await client.Send(new Uri(url), new Models.MessageRequest()
            { 
                Content = $"Testing Direct Message {DateTime.Now}",
                EventName = "TestingEvent"
            });
        }

        private static async void SendQueuedMessage(BroadcasterClientService client)
        {
            var url = "https://localhost:5001/Notifications/queued";
            await client.Send(new Uri(url), new Models.MessageRequest()
            {
                Content = $"Testing Queued Message {DateTime.Now}",
                EventName = "TestingEvent"
            });
        }

        private static void EventListened(object suscriber, BroadcastEventArgs args)
        {
            Console.WriteLine($"New message: {args.MessageRequest.Content}");
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole())
                .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information)
                .AddTransient<BroadcasterClientService>();
        }
    }
}
