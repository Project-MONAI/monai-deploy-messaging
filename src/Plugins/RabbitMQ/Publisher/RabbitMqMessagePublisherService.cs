/*
 * Copyright 2021-2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Common;
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
        private readonly string _useSSL;
        private readonly string _portNumber;
        private bool _disposedValue;

        public string Name => ConfigurationKeys.PublisherServiceName;

        public RabbitMQMessagePublisherService(IOptions<MessageBrokerServiceConfiguration> options,
                                               ILogger<RabbitMQMessagePublisherService> logger,
                                               IRabbitMQConnectionFactory rabbitMqConnectionFactory)
        {
            Guard.Against.Null(options);

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rabbitMqConnectionFactory = rabbitMqConnectionFactory ?? throw new ArgumentNullException(nameof(rabbitMqConnectionFactory));

            var configuration = options.Value;
            ValidateConfiguration(configuration.PublisherSettings);
            _endpoint = configuration.PublisherSettings[ConfigurationKeys.EndPoint];
            _username = configuration.PublisherSettings[ConfigurationKeys.Username];
            _password = configuration.PublisherSettings[ConfigurationKeys.Password];
            _virtualHost = configuration.PublisherSettings[ConfigurationKeys.VirtualHost];
            _exchange = configuration.PublisherSettings[ConfigurationKeys.Exchange];

            if (configuration.PublisherSettings.ContainsKey(ConfigurationKeys.UseSSL))
            {
                _useSSL = configuration.PublisherSettings[ConfigurationKeys.UseSSL];
            }
            else
            {
                _useSSL = String.Empty;
            }

            if (configuration.PublisherSettings.ContainsKey(ConfigurationKeys.Port))
            {
                _portNumber = configuration.PublisherSettings[ConfigurationKeys.Port];
            }
            else
            {
                _portNumber = String.Empty;
            }
        }

        internal static void ValidateConfiguration(Dictionary<string, string> configuration)
        {
            Guard.Against.Null(configuration);

            foreach (var key in ConfigurationKeys.PublisherRequiredKeys)
            {
                if (!configuration.ContainsKey(key))
                {
                    throw new ConfigurationException($"{ConfigurationKeys.PublisherServiceName} is missing configuration for {key}.");
                }
            }
        }

        public Task Publish(string topic, Message message)
        {
            Guard.Against.NullOrWhiteSpace(topic);
            Guard.Against.Null(message);

            using var loggingScope = _logger.BeginScope(new LoggingDataDictionary<string, object>
            {
                ["MessageId"] = message.MessageId,
                ["ApplicationId"] = message.ApplicationId,
                ["CorrelationId"] = message.CorrelationId
            });

            _logger.PublshingRabbitMQ(_endpoint, _virtualHost, _exchange, topic);

            var channel = _rabbitMqConnectionFactory.CreateChannel(ChannelType.Publisher, _endpoint, _username, _password, _virtualHost, _useSSL, _portNumber);

            if (channel is null) { throw new NullReferenceException("RabbitMq channel returned null"); }

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

            channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));

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
