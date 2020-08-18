using Ism.Broadcaster.Enumerations;
using Ism.Broadcaster.EventArgs;
using Ism.Broadcaster.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

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
            EventGroupEnum hubEventName = EventGroupEnum.OnTesting;

            var listener = new BroadcastListenerService(hubURL, hubEventName);
            listener.ListenHubEvent(EventListened);

            var logger = serviceProvider.GetService<ILogger<BroadcasterClientService>>();
            var client = new BroadcasterClientService(logger);

            Console.WriteLine("Press [enter] to send the test message.");
            Console.ReadLine();

            SendDirectMessage(client);

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }

        private static async void SendDirectMessage(BroadcasterClientService client)
        {
            var url = "";
            await client.Send(new Uri(url), new Models.MessageRequest()
            { 
                Content = $"Testing Direct Message {DateTime.Now}",
                EventGroup = EventGroupEnum.Unknown
            });
        }

        private static async void SendQueuedMessage(BroadcasterClientService client)
        {
            var url = "";
            await client.Send(new Uri(url), new Models.MessageRequest()
            {
                Content = $"Testing Queued Message {DateTime.Now}",
                EventGroup = EventGroupEnum.Unknown
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
