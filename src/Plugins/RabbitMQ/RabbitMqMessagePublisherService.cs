// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Globalization;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Messaging.Messages;
using RabbitMQ.Client;

namespace Monai.Deploy.Messaging.RabbitMQ
{
    public class RabbitMQMessagePublisherService : IMessageBrokerPublisherService
    {
        private const int PersistentDeliveryMode = 2;

        private readonly ILogger<RabbitMQMessagePublisherService> _logger;
        private readonly IRabbitMQConnectionFactory _rabbitMqConnectionFactory;
        private readonly string _endpoint;
        private readonly string _username;
        private readonly string _password;
        private readonly string _virtualHost;
        private readonly string _exchange;
        private readonly string _useSSL = string.Empty;
        private readonly string _portNumber = string.Empty;
        private bool _disposedValue;

        public string Name => "Rabbit MQ Publisher";

        public RabbitMQMessagePublisherService(IOptions<MessageBrokerServiceConfiguration> options,
                                               ILogger<RabbitMQMessagePublisherService> logger,
                                               IRabbitMQConnectionFactory rabbitMqConnectionFactory)
        {
            Guard.Against.Null(options, nameof(options));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rabbitMqConnectionFactory = rabbitMqConnectionFactory ?? throw new ArgumentNullException(nameof(rabbitMqConnectionFactory));

            var configuration = options.Value;
            ValidateConfiguration(configuration);
            _endpoint = configuration.PublisherSettings[ConfigurationKeys.EndPoint];
            _username = configuration.PublisherSettings[ConfigurationKeys.Username];
            _password = configuration.PublisherSettings[ConfigurationKeys.Password];
            _virtualHost = configuration.PublisherSettings[ConfigurationKeys.VirtualHost];
            _exchange = configuration.PublisherSettings[ConfigurationKeys.Exchange];

            if (configuration.PublisherSettings.ContainsKey(ConfigurationKeys.UseSSL))
            {
                _useSSL = configuration.PublisherSettings[ConfigurationKeys.UseSSL];
            }

            if (configuration.PublisherSettings.ContainsKey(ConfigurationKeys.Port))
            {
                _portNumber = configuration.PublisherSettings[ConfigurationKeys.Port];
            }
        }

        private void ValidateConfiguration(MessageBrokerServiceConfiguration configuration)
        {
            Guard.Against.Null(configuration, nameof(configuration));
            Guard.Against.Null(configuration.PublisherSettings, nameof(configuration.PublisherSettings));

            foreach (var key in ConfigurationKeys.PublisherRequiredKeys)
            {
                if (!configuration.PublisherSettings.ContainsKey(key))
                {
                    throw new ConfigurationException($"{Name} is missing configuration for {key}.");
                }
            }
        }

        public Task Publish(string topic, Message message)
        {
            Guard.Against.NullOrWhiteSpace(topic, nameof(topic));
            Guard.Against.Null(message, nameof(message));

            using var loggerScope = _logger.BeginScope(string.Format(CultureInfo.InvariantCulture, Logger.LoggingScopeMessageApplication, message.MessageId, message.ApplicationId));

            _logger.PublshingRabbitMQ(_endpoint, _virtualHost, _exchange, topic);

            using var channel = _rabbitMqConnectionFactory.CreateChannel(_endpoint, _username, _password, _virtualHost, _useSSL, _portNumber);
            channel.ExchangeDeclare(_exchange, ExchangeType.Topic, durable: true, autoDelete: false);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = message.ContentType;
            properties.MessageId = message.MessageId;
            properties.AppId = message.ApplicationId;
            properties.CorrelationId = message.CorrelationId;
            properties.DeliveryMode = PersistentDeliveryMode;
            properties.Type = message.MessageDescription;
            properties.Timestamp = new AmqpTimestamp(message.CreationDateTime.ToUnixTimeSeconds());

            channel.BasicPublish(exchange: _exchange,
                                 routingKey: topic,
                                 basicProperties: properties,
                                 body: message.Body);

            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // Dispose any managed objects
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
