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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Messaging.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Monai.Deploy.Messaging.RabbitMQ
{
    public class RabbitMQMessageSubscriberService : IMessageBrokerSubscriberService
    {
        private readonly ILogger<RabbitMQMessageSubscriberService> _logger;
        private readonly string _endpoint;
        private readonly string _virtualHost;
        private readonly string _exchange;
        private readonly string _useSSL;
        private readonly string _portNumber;
        private readonly IModel _channel;
        private bool _disposedValue;

        public string Name => "Rabbit MQ Subscriber";

        public RabbitMQMessageSubscriberService(IOptions<MessageBrokerServiceConfiguration> options,
                                                ILogger<RabbitMQMessageSubscriberService> logger,
                                                IRabbitMQConnectionFactory rabbitMqConnectionFactory)
        {
            Guard.Against.Null(options, nameof(options));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var configuration = options.Value;
            ValidateConfiguration(configuration);
            _endpoint = configuration.SubscriberSettings[ConfigurationKeys.EndPoint];
            var username = configuration.SubscriberSettings[ConfigurationKeys.Username];
            var password = configuration.SubscriberSettings[ConfigurationKeys.Password];
            _virtualHost = configuration.SubscriberSettings[ConfigurationKeys.VirtualHost];
            _exchange = configuration.SubscriberSettings[ConfigurationKeys.Exchange];

            if (configuration.SubscriberSettings.ContainsKey(ConfigurationKeys.UseSSL))
            {
                _useSSL = configuration.SubscriberSettings[ConfigurationKeys.UseSSL];
            }
            else
            {
                _useSSL = String.Empty;
            }

            if (configuration.SubscriberSettings.ContainsKey(ConfigurationKeys.Port))
            {
                _portNumber = configuration.SubscriberSettings[ConfigurationKeys.Port];
            }
            else
            {
                _portNumber = String.Empty;
            }

            _logger.ConnectingToRabbitMQ(Name, _endpoint, _virtualHost);
            _channel = rabbitMqConnectionFactory.CreateChannel(_endpoint, username, password, _virtualHost, _useSSL, _portNumber);
            _channel.ExchangeDeclare(_exchange, ExchangeType.Topic, durable: true, autoDelete: false);
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        }

        private void ValidateConfiguration(MessageBrokerServiceConfiguration configuration)
        {
            Guard.Against.Null(configuration, nameof(configuration));
            Guard.Against.Null(configuration.SubscriberSettings, nameof(configuration.SubscriberSettings));

            foreach (var key in ConfigurationKeys.SubscriberRequiredKeys)
            {
                if (!configuration.SubscriberSettings.ContainsKey(key))
                {
                    throw new ConfigurationException($"{Name} is missing configuration for {key}.");
                }
            }
        }

        public void Subscribe(string topic, string queue, Action<MessageReceivedEventArgs> messageReceivedCallback, ushort prefetchCount = 0)
            => Subscribe(new string[] { topic }, queue, messageReceivedCallback, prefetchCount);

        public void Subscribe(string[] topics, string queue, Action<MessageReceivedEventArgs> messageReceivedCallback, ushort prefetchCount = 0)
        {
            Guard.Against.Null(topics, nameof(topics));
            Guard.Against.Null(messageReceivedCallback, nameof(messageReceivedCallback));

            var queueDeclareResult = _channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false);
            BindToRoutingKeys(topics, queueDeclareResult.QueueName);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, eventArgs) =>
            {
                using var loggerScope = _logger.BeginScope(string.Format(CultureInfo.InvariantCulture, Logger.LoggingScopeMessageApplication, eventArgs.BasicProperties.MessageId, eventArgs.BasicProperties.AppId));

                _logger.MessageReceivedFromQueue(queueDeclareResult.QueueName, eventArgs.RoutingKey);

                MessageReceivedEventArgs messageReceivedEventArgs;
                try
                {
                    messageReceivedEventArgs = CreateMessage(eventArgs.RoutingKey, eventArgs);
                }
                catch (Exception ex)
                {
                    _logger.InvalidMessage(queueDeclareResult.QueueName, eventArgs.RoutingKey, eventArgs.BasicProperties.MessageId, ex);

                    _logger.SendingNAcknowledgement(eventArgs.BasicProperties.MessageId);
                    _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                    _logger.NAcknowledgementSent(eventArgs.BasicProperties.MessageId, false);
                    return;
                }

                try
                {
                    messageReceivedCallback(messageReceivedEventArgs);
                }
                catch (Exception ex)
                {
                    _logger.ErrorNotHandledByCallback(queueDeclareResult.QueueName, eventArgs.RoutingKey, eventArgs.BasicProperties.MessageId, ex);
                }
            };
            _channel.BasicQos(0, prefetchCount, false);
            _channel.BasicConsume(queueDeclareResult.QueueName, false, consumer);
            _logger.SubscribeToRabbitMQQueue(_endpoint, _virtualHost, _exchange, queueDeclareResult.QueueName, string.Join(',', topics));
        }

        public void SubscribeAsync(string topic, string queue, Func<MessageReceivedEventArgs, Task> messageReceivedCallback, ushort prefetchCount = 0)
            => SubscribeAsync(new string[] { topic }, queue, messageReceivedCallback, prefetchCount);

        public void SubscribeAsync(string[] topics, string queue, Func<MessageReceivedEventArgs, Task> messageReceivedCallback, ushort prefetchCount = 0)
        {
            Guard.Against.Null(topics, nameof(topics));
            Guard.Against.Null(messageReceivedCallback, nameof(messageReceivedCallback));

            var queueDeclareResult = _channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false);
            BindToRoutingKeys(topics, queueDeclareResult.QueueName);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, eventArgs) =>
            {
                using var loggerScope = _logger.BeginScope(string.Format(CultureInfo.InvariantCulture, Logger.LoggingScopeMessageApplication, eventArgs.BasicProperties.MessageId, eventArgs.BasicProperties.AppId));

                _logger.MessageReceivedFromQueue(queueDeclareResult.QueueName, eventArgs.RoutingKey);

                MessageReceivedEventArgs messageReceivedEventArgs;
                try
                {
                    messageReceivedEventArgs = CreateMessage(eventArgs.RoutingKey, eventArgs);
                }
                catch (Exception ex)
                {
                    _logger.InvalidMessage(queueDeclareResult.QueueName, eventArgs.RoutingKey, eventArgs.BasicProperties.MessageId, ex);

                    _logger.SendingNAcknowledgement(eventArgs.BasicProperties.MessageId);
                    _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                    _logger.NAcknowledgementSent(eventArgs.BasicProperties.MessageId, false);
                    return;
                }
                try
                {
                    await messageReceivedCallback(messageReceivedEventArgs);
                }
                catch (Exception ex)
                {
                    _logger.ErrorNotHandledByCallback(queueDeclareResult.QueueName, eventArgs.RoutingKey, eventArgs.BasicProperties.MessageId, ex);
                }
            };
            _channel.BasicQos(0, prefetchCount, false);
            _channel.BasicConsume(queueDeclareResult.QueueName, false, consumer);
            _logger.SubscribeToRabbitMQQueue(_endpoint, _virtualHost, _exchange, queueDeclareResult.QueueName, string.Join(',', topics));
        }

        public void Acknowledge(MessageBase message)
        {
            Guard.Against.Null(message, nameof(message));

            _logger.SendingAcknowledgement(message.MessageId);
            _channel.BasicAck(ulong.Parse(message.DeliveryTag, CultureInfo.InvariantCulture), multiple: false);
            _logger.AcknowledgementSent(message.MessageId);
        }

        public void Reject(MessageBase message, bool requeue = true)
        {
            Guard.Against.Null(message, nameof(message));

            _logger.SendingNAcknowledgement(message.MessageId);
            _channel.BasicNack(ulong.Parse(message.DeliveryTag, CultureInfo.InvariantCulture), multiple: false, requeue: requeue);
            _logger.NAcknowledgementSent(message.MessageId, requeue);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _channel.Close();
                    _channel.Dispose();
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

        private void BindToRoutingKeys(string[] topics, string queue)
        {
            Guard.Against.Null(topics, nameof(topics));
            Guard.Against.NullOrWhiteSpace(queue, nameof(queue));

            foreach (var topic in topics)
            {
                if (!string.IsNullOrEmpty(topic))
                {
                    _channel.QueueBind(queue, _exchange, topic);
                }
            }
        }

        private static MessageReceivedEventArgs CreateMessage(string topic, BasicDeliverEventArgs eventArgs)
        {
            Guard.Against.NullOrWhiteSpace(topic, nameof(topic));
            Guard.Against.Null(eventArgs, nameof(eventArgs));

            Guard.Against.Null(eventArgs.Body, nameof(eventArgs.Body));
            Guard.Against.Null(eventArgs.BasicProperties, nameof(eventArgs.BasicProperties));
            Guard.Against.Null(eventArgs.BasicProperties.MessageId, nameof(eventArgs.BasicProperties.MessageId));
            Guard.Against.Null(eventArgs.BasicProperties.AppId, nameof(eventArgs.BasicProperties.AppId));
            Guard.Against.Null(eventArgs.BasicProperties.ContentType, nameof(eventArgs.BasicProperties.ContentType));
            Guard.Against.Null(eventArgs.BasicProperties.CorrelationId, nameof(eventArgs.BasicProperties.CorrelationId));
            Guard.Against.Null(eventArgs.BasicProperties.Timestamp, nameof(eventArgs.BasicProperties.Timestamp));
            Guard.Against.Null(eventArgs.DeliveryTag, nameof(eventArgs.DeliveryTag));

            return new MessageReceivedEventArgs(
                new Message(
                body: eventArgs.Body.ToArray(),
                messageDescription: eventArgs.BasicProperties.Type,
                messageId: eventArgs.BasicProperties.MessageId,
                applicationId: eventArgs.BasicProperties.AppId,
                contentType: eventArgs.BasicProperties.ContentType,
                correlationId: eventArgs.BasicProperties.CorrelationId,
                creationDateTime: DateTimeOffset.FromUnixTimeSeconds(eventArgs.BasicProperties.Timestamp.UnixTime),
                deliveryTag: eventArgs.DeliveryTag.ToString(CultureInfo.InvariantCulture)),
                CancellationToken.None);
        }
    }
}
