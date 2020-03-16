using System;
using System.Net.Http;
using System.Threading.Tasks;
using Ism.RabbitMq.Client.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ism.RabbitMq.Client
{
    public interface IRabbitMqClientService
    {
        IModel Channel { get; }

        Task<HttpResponseMessage> CheckHealth();

        void SetAcknowledge(ulong deliveryTag, bool isPositive);
        void SendDirectLog(string queueName, Message message);
        void SubscribeToDirectLogs(string queueName, Action<Message, ulong> onDirectLogReceivedAction);
    }
}
