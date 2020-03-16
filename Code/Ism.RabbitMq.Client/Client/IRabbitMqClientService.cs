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
        void SendMessage(string queueName, string message);
        void SubscribeToDirectMessages(string queueName, Action<MessageRequest, ulong> onDirectLogReceivedAction);
    }
}
