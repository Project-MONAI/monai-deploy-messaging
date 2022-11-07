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
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly string _deadLetterExchange;
        private readonly int _deliveryLimit;
        private readonly int _requeueDelay;
        private readonly string _useSSL;
        private readonly string _portNumber;
        private readonly IModel _channel;
        private bool _disposedValue;
        private static readonly ConcurrentDictionary<string, DateTime> MessageTimings = new();

        public string Name => ConfigurationKeys.SubscriberServiceName;

        public RabbitMQMessageSubscriberService(IOptions<MessageBrokerServiceConfiguration> options,
                                                ILogger<RabbitMQMessageSubscriberService> logger,
                                                IRabbitMQConnectionFactory rabbitMqConnectionFactory)
        {
            Guard.Against.Null(options);

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var configuration = options.Value;
            ValidateConfiguration(configuration.SubscriberSettings);
            _endpoint = configuration.SubscriberSettings[ConfigurationKeys.EndPoint];
            var username = configuration.SubscriberSettings[ConfigurationKeys.Username];
            var password = configuration.SubscriberSettings[ConfigurationKeys.Password];
            _virtualHost = configuration.SubscriberSettings[ConfigurationKeys.VirtualHost];
            _exchange = configuration.SubscriberSettings[ConfigurationKeys.Exchange];
            _deadLetterExchange = configuration.SubscriberSettings[ConfigurationKeys.DeadLetterExchange];
            _deliveryLimit = int.Parse(configuration.SubscriberSettings[ConfigurationKeys.DeliveryLimit], NumberFormatInfo.InvariantInfo);
            _requeueDelay = int.Parse(configuration.SubscriberSettings[ConfigurationKeys.RequeueDelay], NumberFormatInfo.InvariantInfo);

            if (configuration.SubscriberSettings.ContainsKey(ConfigurationKeys.UseSSL))
            {
                _useSSL = configuration.SubscriberSettings[ConfigurationKeys.UseSSL];
            }
            else
            {
                _useSSL = string.Empty;
            }

            if (configuration.SubscriberSettings.ContainsKey(ConfigurationKeys.Port))
            {
                _portNumber = configuration.SubscriberSettings[ConfigurationKeys.Port];
            }
            else
            {
                _portNumber = string.Empty;
            }

            _logger.ConnectingToRabbitMQ(Name, _endpoint, _virtualHost);
            _channel = rabbitMqConnectionFactory.CreateChannel(_endpoint, username, password, _virtualHost, _useSSL, _portNumber);
            _channel.ExchangeDeclare(_exchange, ExchangeType.Topic, durable: true, autoDelete: false);
            _channel.ExchangeDeclare(_deadLetterExchange, ExchangeType.Topic, durable: true, autoDelete: false);
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        }

        internal static void ValidateConfiguration(Dictionary<string, string> configuration)
        {
            Guard.Against.Null(configuration);

            foreach (var key in ConfigurationKeys.SubscriberRequiredKeys)
            {
                if (!configuration.ContainsKey(key))
                {
                    throw new ConfigurationException($"{ConfigurationKeys.SubscriberServiceName} is missing configuration for {key}.");
                }
            }

            if (!int.TryParse(configuration[ConfigurationKeys.DeliveryLimit], out var deliveryLimit))
            {
                throw new ConfigurationException($"{ConfigurationKeys.SubscriberServiceName} has a non int value for {ConfigurationKeys.DeliveryLimit}");
            }

            if (!int.TryParse(configuration[ConfigurationKeys.RequeueDelay], out var requeueDelay))
            {
                throw new ConfigurationException($"{ConfigurationKeys.SubscriberServiceName} has a non int value for {ConfigurationKeys.RequeueDelay}");
            }

            if (deliveryLimit < 0 || requeueDelay < 0)
            {
                throw new ConfigurationException($"{ConfigurationKeys.SubscriberServiceName} has int values of less than 1");
            }
        }

        public void Subscribe(string topic, string queue, Action<MessageReceivedEventArgs> messageReceivedCallback, ushort prefetchCount = 0)
            => Subscribe(new string[] { topic }, queue, messageReceivedCallback, prefetchCount);

        public void Subscribe(string[] topics, string queue, Action<MessageReceivedEventArgs> messageReceivedCallback, ushort prefetchCount = 0)
        {
            Guard.Against.Null(topics);
            Guard.Against.Null(messageReceivedCallback);

            var arguments = new Dictionary<string, object>()
            {
                { "x-queue-type", "quorum" },
                { "x-delivery-limit", _deliveryLimit },
                { "x-dead-letter-exchange", _deadLetterExchange }
            };

            var deadLetterQueue = $"{queue}-dead-letter";

            var queueDeclareResult = _channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: arguments);
            var deadLetterQueueDeclareResult = _channel.QueueDeclare(queue: deadLetterQueue, durable: true, exclusive: false, autoDelete: false);
            BindToRoutingKeys(topics, queueDeclareResult.QueueName, deadLetterQueueDeclareResult.QueueName);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, eventArgs) =>
            {
                using var loggingScope = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["MessageId"] = eventArgs.BasicProperties.MessageId,
                    ["ApplicationId"] = eventArgs.BasicProperties.AppId,
                    ["CorrelationId"] = eventArgs.BasicProperties.CorrelationId
                });

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
            Guard.Against.Null(topics);
            Guard.Against.Null(messageReceivedCallback);

            var arguments = new Dictionary<string, object>()
            {
                { "x-queue-type", "quorum" },
                { "x-delivery-limit", _deliveryLimit },
                { "x-dead-letter-exchange", _deadLetterExchange }
            };

            var deadLetterQueue = $"{queue}-dead-letter";

            var queueDeclareResult = _channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: arguments);
            var deadLetterQueueDeclareResult = _channel.QueueDeclare(queue: deadLetterQueue, durable: true, exclusive: false, autoDelete: false);
            BindToRoutingKeys(topics, queueDeclareResult.QueueName, deadLetterQueueDeclareResult.QueueName);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, eventArgs) =>
            {
                using var loggingScope = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["MessageId"] = eventArgs.BasicProperties.MessageId,
                    ["ApplicationId"] = eventArgs.BasicProperties.AppId,
                    ["CorrelationId"] = eventArgs.BasicProperties.CorrelationId,
                    ["RecievedTime"] = DateTime.UtcNow
                });

                TimeNewMessage(eventArgs.BasicProperties.MessageId);

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
                    RemoveTimeMessage(eventArgs.BasicProperties.MessageId);
                    return;
                }
                try
                {
                    await messageReceivedCallback(messageReceivedEventArgs).ConfigureAwait(false);
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
            Guard.Against.Null(message);

            _logger.SendingAcknowledgement(message.MessageId);
            _channel.BasicAck(ulong.Parse(message.DeliveryTag, CultureInfo.InvariantCulture), multiple: false);
            var eventDuration = GetMessageDuration(message.MessageId);

            using var loggingScope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["EventDuration"] = eventDuration
            });
            _logger.AcknowledgementSent(message.MessageId, eventDuration);
            RemoveTimeMessage(message.MessageId);
        }

        public async Task RequeueWithDelay(MessageBase message)
        {
            try
            {
                await Task.Delay(_requeueDelay * 1000).ConfigureAwait(false);

                Reject(message, true);
            }
            catch (Exception e)
            {
                _logger.Exception($"Requeue delay failed.", e);
                Reject(message, true);
            }
        }

        public void Reject(MessageBase message, bool requeue = true)
        {
            Guard.Against.Null(message);

            _logger.SendingNAcknowledgement(message.MessageId);
            _channel.BasicNack(ulong.Parse(message.DeliveryTag, CultureInfo.InvariantCulture), multiple: false, requeue: requeue);
            _logger.NAcknowledgementSent(message.MessageId, requeue);
            RemoveTimeMessage(message.MessageId);
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

        private void BindToRoutingKeys(string[] topics, string queue, string deadLetterQueue = "")
        {
            Guard.Against.Null(topics);
            Guard.Against.NullOrWhiteSpace(queue);

            foreach (var topic in topics)
            {
                if (!string.IsNullOrEmpty(topic))
                {
                    _channel.QueueBind(queue, _exchange, topic);

                    if (!string.IsNullOrEmpty(deadLetterQueue))
                    {
                        _channel.QueueBind(deadLetterQueue, _deadLetterExchange, topic);
                    }
                }
            }
        }

        private static MessageReceivedEventArgs CreateMessage(string topic, BasicDeliverEventArgs eventArgs)
        {
            Guard.Against.NullOrWhiteSpace(topic);
            Guard.Against.Null(eventArgs);

            Guard.Against.Null(eventArgs.Body);
            Guard.Against.Null(eventArgs.BasicProperties);
            Guard.Against.Null(eventArgs.BasicProperties.MessageId);
            Guard.Against.Null(eventArgs.BasicProperties.AppId);
            Guard.Against.Null(eventArgs.BasicProperties.ContentType);
            Guard.Against.Null(eventArgs.BasicProperties.CorrelationId);
            Guard.Against.Null(eventArgs.BasicProperties.Timestamp);
            Guard.Against.Null(eventArgs.DeliveryTag);

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

        private static void TimeNewMessage(string messageId)
        {
            if (MessageTimings.ContainsKey(messageId))
            {
                RemoveTimeMessage(messageId);
            }
            MessageTimings.TryAdd(messageId, DateTime.UtcNow);
        }

        private static void RemoveTimeMessage(string messageId)
        {
            if (MessageTimings.ContainsKey(messageId))
            {
                MessageTimings.TryRemove(messageId, out _);
            }
        }

        private static double GetMessageDuration(string messageId)
        {
            if (MessageTimings.ContainsKey(messageId))
            {
                return (DateTime.UtcNow - MessageTimings[messageId]).TotalMilliseconds;
            }
            return 0;
        }
    }
}
