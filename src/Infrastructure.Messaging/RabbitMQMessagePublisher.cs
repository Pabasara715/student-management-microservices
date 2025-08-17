using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polly;
using RabbitMQ.Client;
using Serilog;

namespace StudentSystem.Infrastructure.Messaging
{
    public sealed class RabbitMQMessagePublisher : IMessagePublisher, IDisposable
    {
        private const int DEFAULT_PORT = 5672;
        private const string DEFAULT_VIRTUAL_HOST = "/";

        private readonly List<string> _hosts;
        private readonly string _virtualHost;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly string _exchange;
        private IConnection _connection;
        private IModel _channel; 
        private bool _disposed;

        public RabbitMQMessagePublisher(string host, string username, string password, string exchange, int port)
            : this(new List<string>() { host }, DEFAULT_VIRTUAL_HOST, username, password, exchange, port)
        {
        }

        public RabbitMQMessagePublisher(string host, string virtualHost, string username, string password, string exchange)
            : this(new List<string>() { host }, virtualHost, username, password, exchange, DEFAULT_PORT)
        {
        }

        public RabbitMQMessagePublisher(string host, string virtualHost, string username, string password, string exchange, int port)
            : this(new List<string>() { host }, virtualHost, username, password, exchange, port)
        {
        }

        public RabbitMQMessagePublisher(string host, string username, string password, string exchange)
            : this(new List<string>() { host }, DEFAULT_VIRTUAL_HOST, username, password, exchange, DEFAULT_PORT)
        {
        }

        public RabbitMQMessagePublisher(IEnumerable<string> hosts, string username, string password, string exchange)
            : this(hosts, DEFAULT_VIRTUAL_HOST, username, password, exchange, DEFAULT_PORT)
        {
        }

        public RabbitMQMessagePublisher(IEnumerable<string> hosts, string virtualHost, string username, string password, string exchange, int port)
        {
            _hosts = hosts.ToList();
            _port = port;
            _virtualHost = virtualHost;
            _username = username;
            _password = password;
            _exchange = exchange;
            Connect();
        }

        public async Task PublishMessageAsync(string messageType, object message, string routingKey)
        {
            string data = MessageSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(data);
            IBasicProperties properties = _channel.CreateBasicProperties(); // Works in RabbitMQ.Client 6.5.0
            properties.Headers = new Dictionary<string, object> { { "MessageType", messageType } };
            _channel.BasicPublish(_exchange, routingKey, mandatory: false, basicProperties: properties, body: body);
        }

        private void Connect()
        {
            Policy
                .Handle<Exception>()
                .WaitAndRetry(9, r => TimeSpan.FromSeconds(5), (ex, ts) => { Log.Error("Error connecting to RabbitMQ. Retrying in 5 sec."); })
                .Execute(() =>
                {
                    var factory = new ConnectionFactory() { VirtualHost = _virtualHost, UserName = _username, Password = _password, Port = _port };
                    factory.AutomaticRecoveryEnabled = true;
                    _connection = factory.CreateConnection(_hosts); 
                    _channel = _connection.CreateModel();
                    _channel.ExchangeDeclare(_exchange, "fanout", durable: true, autoDelete: false);
                });
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
    }
}