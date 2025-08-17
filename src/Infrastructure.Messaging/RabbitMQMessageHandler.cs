using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace StudentSystem.Infrastructure.Messaging
{
    public class RabbitMQMessageHandler : IMessageHandler
    {
        private const int DEFAULT_PORT = 5672;
        private const string DEFAULT_VIRTUAL_HOST = "/";

        private readonly List<string> _hosts;
        private readonly string _username;
        private readonly string _virtualHost;
        private readonly string _password;
        private readonly string _exchange;
        private readonly string _queueName;
        private readonly string _routingKey;
        private readonly int _port;
        private IConnection _connection;
        private IModel _channel;
        private AsyncEventingBasicConsumer _consumer;
        private string _consumerTag;
        private IMessageHandlerCallback _callback;

        public RabbitMQMessageHandler(string host, string username, string password,
            string exchange, string queueName, string routingKey)
            : this(new List<string> { host }, DEFAULT_VIRTUAL_HOST, username, password,
                exchange, queueName, routingKey, DEFAULT_PORT)
        {
        }

        public RabbitMQMessageHandler(string host, string virtualHost, string username, string password,
            string exchange, string queueName, string routingKey)
            : this(new List<string> { host }, virtualHost, username, password,
                exchange, queueName, routingKey, DEFAULT_PORT)
        {
        }

        public RabbitMQMessageHandler(string host, string username, string password,
            string exchange, string queueName, string routingKey, int port)
            : this(new List<string> { host }, DEFAULT_VIRTUAL_HOST, username, password,
                exchange, queueName, routingKey, port)
        {
        }

        public RabbitMQMessageHandler(string host, string virtualHost, string username, string password,
            string exchange, string queueName, string routingKey, int port)
            : this(new List<string> { host }, virtualHost, username, password,
                exchange, queueName, routingKey, port)
        {
        }

        public RabbitMQMessageHandler(IEnumerable<string> hosts, string username, string password,
            string exchange, string queueName, string routingKey)
            : this(hosts, DEFAULT_VIRTUAL_HOST, username, password,
                exchange, queueName, routingKey, DEFAULT_PORT)
        {
        }

        public RabbitMQMessageHandler(IEnumerable<string> hosts, string virtualHost, string username, string password,
            string exchange, string queueName, string routingKey, int port)
        {
            _hosts = hosts.ToList();
            _virtualHost = virtualHost;
            _port = port;
            _username = username;
            _password = password;
            _exchange = exchange;
            _queueName = queueName;
            _routingKey = routingKey;
        }

        public void Start(IMessageHandlerCallback callback)
        {
            _callback = callback;
            Policy
                .Handle<Exception>()
                .WaitAndRetry(9, r => TimeSpan.FromSeconds(5), (ex, ts) => { Log.Error("Error connecting to RabbitMQ. Retrying in 5 sec."); })
                .Execute(() =>
                {
                    var factory = new ConnectionFactory() { VirtualHost = _virtualHost, UserName = _username, Password = _password, Port = _port };
                    _connection = factory.CreateConnection(_hosts); 
                    _channel = _connection.CreateModel();
                    _channel.ExchangeDeclare(_exchange, "fanout", durable: true, autoDelete: false);
                    _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false);
                    _channel.QueueBind(queue: _queueName, exchange: _exchange, routingKey: _routingKey);
                    _consumer = new AsyncEventingBasicConsumer(_channel);
                    _consumer.Received += Consumer_Received;
                    _consumerTag = _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: _consumer);
                });
        }

        public void Stop()
        {
            if (_consumerTag != null)
            {
                _channel.BasicCancel(_consumerTag);
            }
            _channel?.Close(200, "Goodbye");
            _connection?.Close();
        }
        private async Task Consumer_Received(object sender, BasicDeliverEventArgs ea)
        {
            try
            {
                if (await HandleEvent(ea))
                {
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling message in Consumer_Received");
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        }

        private Task<bool> HandleEvent(BasicDeliverEventArgs ea)
        {
            string messageType = Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["MessageType"]);
            string body = Encoding.UTF8.GetString(ea.Body.ToArray());
            return _callback.HandleMessageAsync(messageType, body);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _channel?.Close();
            _channel?.Dispose();
            _channel = null;
            _connection?.Close();
            _connection?.Dispose();
            _connection = null;
            _disposed = true;
        }

        private bool _disposed;

        ~RabbitMQMessageHandler()
        {
            Dispose();
        }
    }
}