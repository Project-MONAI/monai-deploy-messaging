// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Globalization;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Messaging.Messages;
using RabbitMQ.Client;

namespace Monai.Deploy.Messaging.RabbitMq
{
    public class RabbitMqMessagePublisherService : IMessageBrokerPublisherService
    {
        private const int PersistentDeliveryMode = 2;

        private readonly ILogger<RabbitMqMessagePublisherService> _logger;
        private readonly IRabbitMqConnectionFactory _rabbitMqConnectionFactory;
        private readonly string _endpoint;
        private readonly string _username;
        private readonly string _password;
        private readonly string _virtualHost;
        private readonly string _exchange;
        private bool _disposedValue;

        public string Name => "Rabbit MQ Publisher";

        public RabbitMqMessagePublisherService(IOptions<MessageBrokerServiceConfiguration> options,
                                               ILogger<RabbitMqMessagePublisherService> logger,
                                               IRabbitMqConnectionFactory rabbitMqConnectionFactory)
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

            using var loggerScope = _logger.BeginScope(string.Format(CultureInfo.InvariantCulture, Log.LoggingScopeMessageApplication, message.MessageId, message.ApplicationId));

            _logger.PublshingRabbitMq(_endpoint, _virtualHost, _exchange, topic);

            using var channel = _rabbitMqConnectionFactory.CreateChannel(_endpoint, _username, _password, _virtualHost);
            channel.ExchangeDeclare(_exchange, ExchangeType.Topic);

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
