using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Ism.RabbitMq.Client.Enumerations;
using Ism.RabbitMq.Client.Helpers;
using Ism.RabbitMq.Client.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Ism.RabbitMq.Client.Extensions;

namespace Ism.RabbitMq.Client
{
    public class RabbitMqClientService : IRabbitMqClientService
    {
        #region private fields

        readonly string _hostName;
        readonly int _port;
        readonly int _managementPort;
        readonly string _userName;
        readonly string _password;

        readonly IConnection connection;
        readonly ConnectionFactory factory;

        public IModel Channel { get; internal set; }

        private Action<MessageRequest, ulong> OnMessageReceived { get; set; }
        private Action<MessageRequest> OnFanoutLogReceived { get; set; }
        private Action<MessageRequest> OnBasicMessageReceived { get; set; }

        #endregion

        public RabbitMqClientService(string hostName, 
                                        int port, int managementPort, 
                                        string userName, 
                                        string password)
        {

                _hostName = hostName;
                _port = port;
                _managementPort = managementPort;
                _userName = userName;
                _password = password;

                Console.WriteLine($"Starting connection of RabbitMQ Client. hostName '{hostName}' userName '{userName}' password '{password}'");

                factory = new ConnectionFactory()
                {
                    HostName = _hostName,
                    Port = _port,
                    UserName = _userName,
                    Password = _password
                };

                connection = factory.CreateConnection();
                Channel = connection.CreateModel();
        }

        public void SetAcknowledge(ulong deliveryTag, bool isPositive)
        {
            if (isPositive)
                Channel.BasicAck(deliveryTag, false);
            else
                Channel.BasicNack(deliveryTag, false, true);
        }

        public async Task<HttpResponseMessage> CheckHealth()
        {
            var uri = new Uri($"http://{_hostName}:{_managementPort}/api/overview");
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Basic",
                            Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", _userName, _password))));

                    var response = await client.GetAsync(uri);
                    return response;
                }
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(CustomErrorsHelper.GetError(HealthCheckStatus.RabbitMqNotAvailable, uri.OriginalString, ex.Message, ex.StackTrace), Encoding.UTF8, "application/json")
                };
            }
        }

        public void SendMessage(string queueName, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            Channel.ExchangeDeclare(exchange: queueName, type: "direct");

            var properties = Channel.CreateBasicProperties();
            properties.Persistent = true;

            Channel.BasicPublish(exchange: queueName, routingKey: queueName, basicProperties: properties, body: body);
        }

        public void SubscribeToDirectMessages(string queueName, Action<MessageRequest, ulong> onDirectMessageReceivedAction)
        {
            this.OnMessageReceived = onDirectMessageReceivedAction;

            var _subscribeModel = Channel.QueueDeclare(queue: queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

            Console.WriteLine($"Queue {queueName} was declared and has {_subscribeModel.MessageCount} messages");

            Channel.ExchangeDeclare(exchange: queueName, type: "direct");
            Channel.QueueBind(queue: queueName, exchange: queueName, routingKey: queueName);

            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += (sender, args) =>
            {
                var body = args.Body;
                var message = Encoding.UTF8.GetString(body);
                var routingKey = args.RoutingKey;

                OnMessageReceived?.Invoke(message.Get<MessageRequest>(), args.DeliveryTag);
            };

            Channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }

        public void SendBasicMessage(MessageRequest message)
        {
            Channel.QueueDeclare(queue: "telemetry", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var body = Encoding.UTF8.GetBytes(message.Json());

            Channel.BasicPublish(exchange: "", routingKey: "telemetry", basicProperties: null, body: body);
        }

        public void SubscribeToBasicMessages(Action<MessageRequest> onBasicMessageReceivedAction)
        {
            OnBasicMessageReceived = onBasicMessageReceivedAction;
            Channel.QueueDeclare(queue: "telemetry", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                OnBasicMessageReceived?.Invoke(message.Get<MessageRequest>());
            };

            Channel.BasicConsume(queue: "telemetry", autoAck: true, consumer: consumer);
        }

        public void SendFanoutMessage(MessageRequest serializedMessage)
        {
            var body = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(serializedMessage));

            Channel.ExchangeDeclare(exchange: "logs", type: "fanout");
            Channel.BasicPublish(exchange: "logs", routingKey: "", basicProperties: null, body: body);
        }

        public void SubscribeToFanoutsMessage(Action<MessageRequest> onFanoutLogReceivedAction)
        {
            OnFanoutLogReceived = onFanoutLogReceivedAction;

            Channel.ExchangeDeclare(exchange: "logs", type: "fanout");

            var queueName = Channel.QueueDeclare().QueueName;
            Channel.QueueBind(queue: queueName, exchange: "logs", routingKey: "");

            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                OnFanoutLogReceived?.Invoke(message.Get<MessageRequest>());
            };

            Channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }
    }
}
