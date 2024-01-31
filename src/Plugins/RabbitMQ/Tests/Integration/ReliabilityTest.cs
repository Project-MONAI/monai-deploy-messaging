/*
 * Copyright 2022 MONAI Consortium
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

using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Messaging.Messages;
using Moq;
using Xunit;

namespace Monai.Deploy.Messaging.RabbitMQ.Tests.Integration
{
    /// <summary>
    /// This test creates {Channels} queues/topics, {Channel} publishers and randomly sends {MessageCount} messages using
    /// one of the publishers to a randomly selected topic using {MaxDegreeOfParallelism} threads.
    /// </summary>
    public class ReliabilityTest : IDisposable
    {
        private static readonly int MessageCount = 10000;
        private static readonly int MaxDegreeOfParallelism = 10;
        private static readonly int Channels = 10;
        private readonly Mock<ILogger<RabbitMQConnectionFactory>> _logger;
        private readonly Mock<ILogger<RabbitMQMessagePublisherService>> _loggerPublisher;
        private readonly Mock<ILogger<RabbitMQMessageSubscriberService>> _loggerSubscriber;
        private readonly RabbitMQConnectionFactory _factory;
        private readonly List<IMessageBrokerPublisherService> _publishers;
        private readonly List<IMessageBrokerSubscriberService> _subscribers;
        private readonly IOptions<MessageBrokerServiceConfiguration> _options;
        private readonly List<string> _topics;
        private readonly ConcurrentDictionary<string, int> _messages;
        private readonly Random _random;
        private bool _disposedValue;

        public ReliabilityTest()
        {
            _logger = new Mock<ILogger<RabbitMQConnectionFactory>>();
            _loggerPublisher = new Mock<ILogger<RabbitMQMessagePublisherService>>();
            _loggerSubscriber = new Mock<ILogger<RabbitMQMessageSubscriberService>>();
            _factory = new RabbitMQConnectionFactory(_logger.Object);

            _publishers = [];
            _subscribers = [];

            _options = Options.Create(new MessageBrokerServiceConfiguration());
            _options.Value.PublisherSettings[ConfigurationKeys.EndPoint] = RabbitMqConnection.HostName;
            _options.Value.PublisherSettings[ConfigurationKeys.Username] = RabbitMqConnection.Username;
            _options.Value.PublisherSettings[ConfigurationKeys.Password] = RabbitMqConnection.Password;
            _options.Value.PublisherSettings[ConfigurationKeys.VirtualHost] = RabbitMqConnection.VirtualHost;
            _options.Value.PublisherSettings[ConfigurationKeys.Exchange] = RabbitMqConnection.Exchange;
            _options.Value.SubscriberSettings[ConfigurationKeys.EndPoint] = RabbitMqConnection.HostName;
            _options.Value.SubscriberSettings[ConfigurationKeys.Username] = RabbitMqConnection.Username;
            _options.Value.SubscriberSettings[ConfigurationKeys.Password] = RabbitMqConnection.Password;
            _options.Value.SubscriberSettings[ConfigurationKeys.VirtualHost] = RabbitMqConnection.VirtualHost;
            _options.Value.SubscriberSettings[ConfigurationKeys.Exchange] = RabbitMqConnection.Exchange;
            _options.Value.SubscriberSettings[ConfigurationKeys.DeadLetterExchange] = RabbitMqConnection.DeadLetterExchange;
            _options.Value.SubscriberSettings[ConfigurationKeys.DeliveryLimit] = RabbitMqConnection.DeliveryLimit;
            _options.Value.SubscriberSettings[ConfigurationKeys.RequeueDelay] = RabbitMqConnection.RequeueDelay;

            _topics = [];
            _messages = new ConcurrentDictionary<string, int>();
            _random = new Random();
            SetupTopics();
            SetupPublishers();
            SetupSubscribers();
        }

        private void SetupSubscribers()
        {
            for (var i = 0; i < Channels; i++)
            {
                var subscriber = new RabbitMQMessageSubscriberService(_options, _loggerSubscriber.Object, _factory);
                _subscribers.Add(subscriber);
            }
        }

        private void SetupPublishers()
        {
            for (var i = 0; i < Channels; i++)
            {
                _publishers.Add(new RabbitMQMessagePublisherService(_options, _loggerPublisher.Object, _factory));
            }
        }

        private void SetupTopics()
        {
            for (var i = 0; i < Channels; i++)
            {
                var topic = $"mdtest-{i}";
                _topics.Add(topic);
                var channel = _factory.CreateChannel(ChannelType.Publisher, RabbitMqConnection.HostName, RabbitMqConnection.Username, RabbitMqConnection.Password, RabbitMqConnection.VirtualHost, RabbitMqConnection.UseSsl, RabbitMqConnection.PortNumber);

                try
                {
                    channel.QueuePurge(topic);
                }
                catch
                {
                    //noop
                }

                try
                {
                    channel.QueuePurge($"{topic}-dead-letter");
                }
                catch
                {
                    //noop
                }
            }
        }

        [Fact]
        public void GivenMessages_WhenPublished_SubscribeShallReceiveAndAckMessages()
        {
            var countDownEvent = new CountdownEvent(MessageCount);
            for (var i = 0; i < Channels; i++)
            {
                var subscriber = _subscribers[i];
                var topic = _topics[i];

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
                subscriber.SubscribeAsync(topic, _topics[i], async Task (Common.MessageReceivedEventArgs args) =>
                {
                    subscriber.Acknowledge(args.Message);
                    Assert.True(_messages.TryUpdate($"{args.Message.MessageDescription}-{args.Message.MessageId}", 1, 0));
                    countDownEvent.Signal();
                });
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            }

            Parallel.For(0, MessageCount, new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism }, i =>
                {
                    var guid = Guid.NewGuid().ToString();
                    var topic = _topics[_random.Next(Channels)];
                    Assert.True(_messages.TryAdd($"{topic}-{guid}", 0));
                    _publishers[_random.Next(Channels)].Publish(topic, new Message(Encoding.UTF8.GetBytes(guid), topic, guid, guid, guid, guid, DateTimeOffset.UtcNow));
                });

            countDownEvent.Wait(TimeSpan.FromMinutes(1));

            var count = _messages.Values.Count(p => p == 1);
            Assert.Equal(MessageCount, _messages.Count(p => p.Value == 1));
        }

        [Fact]
        public void GivenMessages_WhenPublished_SubscribeAsyncShallReceiveAndAckMessages()
        {
            var countDownEvent = new CountdownEvent(MessageCount);
            for (var i = 0; i < Channels; i++)
            {
                var subscriber = _subscribers[i];
                var topic = _topics[i];
                subscriber.SubscribeAsync(topic, _topics[i], (args) =>
                {
                    subscriber.Acknowledge(args.Message);
                    Assert.True(_messages.TryUpdate($"{args.Message.MessageDescription}-{args.Message.MessageId}", 1, 0));
                    countDownEvent.Signal();
                    return Task.CompletedTask;
                });
            }

            Parallel.For(0, MessageCount, new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism }, i =>
            {
                var guid = Guid.NewGuid().ToString();
                var topic = _topics[_random.Next(Channels)];
                Assert.True(_messages.TryAdd($"{topic}-{guid}", 0));
                _publishers[_random.Next(Channels)].Publish(topic, new Message(Encoding.UTF8.GetBytes(guid), topic, guid, guid, guid, guid, DateTimeOffset.UtcNow));
            });

            countDownEvent.Wait(TimeSpan.FromMinutes(1));

            Assert.True(_messages.Values.All(p => p == 1));
        }

        [Fact]
        public void GivenMessages_WhenPublished_SubscribeAsyncShallReceiveAndNackMessages()
        {
            var countDownEvent = new CountdownEvent(MessageCount);
            for (var i = 0; i < Channels; i++)
            {
                var subscriber = _subscribers[i];
                var topic = _topics[i];
                subscriber.SubscribeAsync(topic, _topics[i], (args) =>
                {
                    subscriber.Reject(args.Message);
                    Assert.True(_messages.TryUpdate($"{args.Message.MessageDescription}-{args.Message.MessageId}", 1, 0));
                    countDownEvent.Signal();
                    return Task.CompletedTask;
                });
            }

            Parallel.For(0, MessageCount, new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism }, i =>
            {
                var guid = Guid.NewGuid().ToString();
                var topic = _topics[_random.Next(Channels)];
                Assert.True(_messages.TryAdd($"{topic}-{guid}", 0));
                _publishers[_random.Next(Channels)].Publish(topic, new Message(Encoding.UTF8.GetBytes(guid), topic, guid, guid, guid, guid, DateTimeOffset.UtcNow));
            });

            countDownEvent.Wait(TimeSpan.FromMinutes(1));

            Assert.True(_messages.Values.All(p => p == 1));
        }

        [Fact]
        public void GivenMessages_WhenPublished_SubscribeAsyncShallRejectRequeueAndThenAckMessages()
        {
            var countDownEvent = new CountdownEvent(MessageCount);
            for (var i = 0; i < Channels; i++)
            {
                var subscriber = _subscribers[i];
                var topic = _topics[i];
                subscriber.SubscribeAsync(topic, _topics[i], (args) =>
                {
                    if (_messages.TryUpdate($"{args.Message.MessageDescription}-{args.Message.MessageId}", 1, 0))
                    {
                        subscriber.Reject(args.Message, true);
                    }
                    else
                    {
                        Assert.True(_messages.TryUpdate($"{args.Message.MessageDescription}-{args.Message.MessageId}", 2, 1));
                        subscriber.Acknowledge(args.Message);
                        countDownEvent.Signal();
                    }
                    return Task.CompletedTask;
                });
            }

            Parallel.For(0, MessageCount, new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism }, i =>
            {
                var guid = Guid.NewGuid().ToString();
                var topic = _topics[_random.Next(Channels)];
                Assert.True(_messages.TryAdd($"{topic}-{guid}", 0));
                _publishers[_random.Next(Channels)].Publish(topic, new Message(Encoding.UTF8.GetBytes(guid), topic, guid, guid, guid, guid, DateTimeOffset.UtcNow));
            });

            countDownEvent.Wait(TimeSpan.FromMinutes(1));

            Assert.True(_messages.Values.All(p => p == 2));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    for (var i = 0; i < Channels; i++)
                    {
                        _publishers[i].Dispose();
                        _subscribers[i].Dispose();
                        _factory.Dispose();
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
    }
}
