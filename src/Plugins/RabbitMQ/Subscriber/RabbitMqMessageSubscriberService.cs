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
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Monai.Deploy.Messaging.RabbitMQ
{
    public class RabbitMQMessageSubscriberService : IMessageBrokerSubscriberService
    {
        private readonly ILogger<RabbitMQMessageSubscriberService> _logger;
        private readonly IRabbitMQConnectionFactory _rabbitMqConnectionFactory;
        private readonly string _endpoint;
        private readonly string _username;
        private readonly string _password;
        private readonly string _virtualHost;
        private readonly string _exchange;
        private readonly string _deadLetterExchange;
        private readonly int _deliveryLimit;
        private readonly int _requeueDelay;
        private readonly string _useSSL;
        private readonly string _portNumber;
        private IModel? _channel;
        private bool _disposedValue;

        public event ConnectionErrorHandler? OnConnectionError;

        public string Name => ConfigurationKeys.SubscriberServiceName;

        public RabbitMQMessageSubscriberService(IOptions<MessageBrokerServiceConfiguration> options,
                                                ILogger<RabbitMQMessageSubscriberService> logger,
                                                IRabbitMQConnectionFactory rabbitMqConnectionFactory)
        {
            Guard.Against.Null(options, nameof(options));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rabbitMqConnectionFactory = rabbitMqConnectionFactory ?? throw new ArgumentNullException(nameof(rabbitMqConnectionFactory));
            var configuration = options.Value;
            ValidateConfiguration(configuration.SubscriberSettings);
            _endpoint = configuration.SubscriberSettings[ConfigurationKeys.EndPoint];
            _username = configuration.SubscriberSettings[ConfigurationKeys.Username];
            _password = configuration.SubscriberSettings[ConfigurationKeys.Password];
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

            CreateChannel();
        }

        private void CreateChannel()
        {
            if (_channel is null || _channel.IsClosed)
            {
                Policy
                    .Handle<Exception>()
                    .WaitAndRetryForever(
                        sleepDurationProvider: attempt => TimeSpan.FromSeconds(1),
                        onRetry: (exception, attempt, waitDuration) =>
                        {
                            _logger.ErrorEstablishConnection(attempt, exception);
                        })
                    .Execute(() =>
                    {
                        _logger.ConnectingToRabbitMQ(Name, _endpoint, _virtualHost);
                        _channel = _rabbitMqConnectionFactory.CreateChannel(ChannelType.Subscriber, _endpoint, _username, _password, _virtualHost, _useSSL, _portNumber) ?? throw new ServiceException("Failed to create a new channel to RabbitMQ");
                        _channel.ExchangeDeclare(_exchange, ExchangeType.Topic, durable: true, autoDelete: false);
                        _channel.ExchangeDeclare(_deadLetterExchange, ExchangeType.Topic, durable: true, autoDelete: false);
                        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                        _channel.ModelShutdown += Channel_ModelShutdown;
                        _logger.ConnectedToRabbitMQ(Name, _endpoint, _virtualHost);
                    });
            }
        }

        private void Channel_ModelShutdown(object? sender, ShutdownEventArgs e)
        {
            if (e.Initiator != ShutdownInitiator.Application)
            {
                if (OnConnectionError is not null)
                {
                    _logger.NotifyModelShutdown(e.ToString());
                    OnConnectionError(sender, new ConnectionErrorArgs(e.ToString()));
                }
            }
            else
            {
                _logger.DetectedChannelShutdownDueToApplicationEvent(e.ToString());
            }
        }

        internal static void ValidateConfiguration(Dictionary<string, string> configuration)
        {
            Guard.Against.Null(configuration, nameof(configuration));

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

        public void SubscribeAsync(string topic, string queue, Func<MessageReceivedEventArgs, Task> messageReceivedCallback, ushort prefetchCount = 0)
            => SubscribeAsync(new string[] { topic }, queue, messageReceivedCallback, prefetchCount);

        public void SubscribeAsync(string[] topics, string queue, Func<MessageReceivedEventArgs, Task> messageReceivedCallback, ushort prefetchCount = 0)
        {
            Guard.Against.Null(topics, nameof(topics));
            Guard.Against.Null(messageReceivedCallback, nameof(messageReceivedCallback));

            var queueDeclareResult = DeclareQueues(topics, queue, prefetchCount);
            var consumer = CreateConsumer(messageReceivedCallback, queueDeclareResult);

            _channel.BasicConsume(queueDeclareResult.QueueName, false, consumer);
            _logger.SubscribeToRabbitMQQueue(_endpoint, _virtualHost, _exchange, queueDeclareResult.QueueName, string.Join(',', topics));
        }

        private EventingBasicConsumer CreateConsumer(Func<MessageReceivedEventArgs, Task> messageReceivedCallback, QueueDeclareOk queueDeclareResult)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, eventArgs) =>
            {
                using var loggingScope = _logger.BeginScope(new LoggingDataDictionary<string, object>
                {
                    ["@messageId"] = eventArgs.BasicProperties.MessageId,
                    ["@applicationId"] = eventArgs.BasicProperties.AppId,
                    ["@correlationId"] = eventArgs.BasicProperties.CorrelationId,
                    ["@recievedTime"] = DateTime.UtcNow
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
                    _channel!.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                    _logger.NAcknowledgementSent(eventArgs.BasicProperties.MessageId, false);
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
            return consumer;
        }

        private QueueDeclareOk DeclareQueues(string[] topics, string queue, ushort prefetchCount)
        {
            var arguments = new Dictionary<string, object>()
            {
                { "x-queue-type", "quorum" },
                { "x-delivery-limit", _deliveryLimit },
                { "x-dead-letter-exchange", _deadLetterExchange }
            };

            var deadLetterQueue = $"{queue}-dead-letter";

            CreateChannel();

            var queueDeclareResult = _channel!.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: arguments);

            var deadLetterExists = QueueExists(deadLetterQueue);
            if (deadLetterExists.exists == false)
            {
                _channel.QueueDeclare(queue: deadLetterQueue, durable: true, exclusive: false, autoDelete: false);
            }

            try
            {
                BindToRoutingKeys(topics, queueDeclareResult.QueueName, deadLetterExists.accessable ? deadLetterQueue : "");
                _channel.BasicQos(0, prefetchCount, false);
            }
            catch (OperationInterruptedException operationInterruptedException)
            {
                //RabbitMQ node that hosts the previously created dead-letter queue is unavailable
                if (operationInterruptedException.Message.Contains("down or inaccessible"))
                {
                    _logger.DetectedInaccessibleNodeThatHousesDeadLetterQueue(deadLetterQueue);
                }
                else
                {
                    //Throw if this generic exception type is used for another reason
                    throw;
                }
            }

            return queueDeclareResult;
        }

        public void Acknowledge(MessageBase message)
        {
            Guard.Against.Null(message, nameof(message));

            CreateChannel();

            _logger.SendingAcknowledgement(message.MessageId);
            _channel!.BasicAck(ulong.Parse(message.DeliveryTag, CultureInfo.InvariantCulture), multiple: false);
            var eventDuration = (DateTime.UtcNow - message.CreationDateTime).TotalMilliseconds;

            using var loggingScope = _logger.BeginScope(new LoggingDataDictionary<string, object>
            {
                ["@eventDuration"] = eventDuration
            });
            _logger.AcknowledgementSent(message.MessageId, eventDuration);
        }

        public async Task RequeueWithDelay(MessageBase message)
        {
            Guard.Against.Null(message, nameof(message));

            try
            {
                await Task.Delay(_requeueDelay * 1000).ConfigureAwait(false);

                Reject(message, true);
            }
            catch (Exception e)
            {
                _logger.ErrorRequeue($"Requeue delay failed.", e);
                Reject(message, true);
            }
        }

        public void Reject(MessageBase message, bool requeue = true)
        {
            Guard.Against.Null(message, nameof(message));

            CreateChannel();

            _logger.SendingNAcknowledgement(message.MessageId);
            _channel!.BasicNack(ulong.Parse(message.DeliveryTag, CultureInfo.InvariantCulture), multiple: false, requeue: requeue);
            _logger.NAcknowledgementSent(message.MessageId, requeue);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        _channel?.Close();
                        _channel?.Dispose();
                    }
                    catch
                    {
                        // ignore
                    }
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
            Guard.Against.Null(topics, nameof(topics));
            Guard.Against.NullOrWhiteSpace(queue, nameof(queue));

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

        private (bool exists, bool accessable) QueueExists(string queueName)
        {
            var testChannel = _rabbitMqConnectionFactory.MakeTempChannel(ChannelType.Subscriber, _endpoint, _username, _password, _virtualHost, _useSSL, _portNumber);

            try
            {
                var testRun = testChannel!.QueueDeclarePassive(queue: queueName);
            }
            catch (OperationInterruptedException operationInterruptedException)
            {
                // RabbitMQ node that hosts the previously created dead-letter queue is unavailable
                if (operationInterruptedException.Message.Contains("down or inaccessible"))
                {
                    _logger.DetectedInaccessibleNodeThatHousesDeadLetterQueue(queueName);
                    return (true, false);
                }
                else
                {
                    return (false, true);
                }
            }
            return (true, true);
        }
    }
}
