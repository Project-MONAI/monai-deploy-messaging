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

using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Messaging.Messages;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace Monai.Deploy.Messaging.RabbitMQ.Tests
{
    public class RabbitMQMessageSubscriberServiceTest
    {
        private readonly IOptions<MessageBrokerServiceConfiguration> _options;
        private readonly Mock<ILogger<RabbitMQMessageSubscriberService>> _logger;
        private readonly Mock<IRabbitMQConnectionFactory> _connectionFactory;
        private readonly Mock<IModel> _model;

        public RabbitMQMessageSubscriberServiceTest()
        {
            _options = Options.Create(new MessageBrokerServiceConfiguration());
            _logger = new Mock<ILogger<RabbitMQMessageSubscriberService>>();
            _connectionFactory = new Mock<IRabbitMQConnectionFactory>();
            _model = new Mock<IModel>();

            _connectionFactory.Setup(p => p.CreateChannel(It.IsAny<ChannelType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_model.Object);
        }

        [Fact(DisplayName = "Fails to validate when required keys are missing")]
        public void FailsToValidateWhenRequiredKeysAreMissing()
        {
            Assert.Throws<ConfigurationException>(() => new RabbitMQMessageSubscriberService(_options, _logger.Object, _connectionFactory.Object));
        }

        [Fact(DisplayName = "Cleanup connections on Dispose")]
        public void CleanupOnDispose()
        {
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.EndPoint, "endpoint");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Username, "username");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Password, "password");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.VirtualHost, "virtual-host");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Exchange, "exchange");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.DeadLetterExchange, "exchange-dead-letter");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.DeliveryLimit, "3");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.RequeueDelay, "30");

            var service = new RabbitMQMessageSubscriberService(_options, _logger.Object, _connectionFactory.Object);
            service.Dispose();

            _model.Verify(p => p.Close(), Times.Once());
            _model.Verify(p => p.Dispose(), Times.Once());
        }

        static Message? s_messageReceived;

        [Fact(DisplayName = "Subscribes to a topic")]
        public void SubscribesToATopic()
        {
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.EndPoint, "endpoint");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Username, "username");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Password, "password");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.VirtualHost, "virtual-host");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Exchange, "exchange");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.DeadLetterExchange, "exchange-dead-letter");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.DeliveryLimit, "3");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.RequeueDelay, "30");

            var jsonMessage = new JsonMessage<string>("hello world", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "1");
            var message = jsonMessage.ToMessage();

            var creationTime = DateTimeOffset.FromUnixTimeSeconds((new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()));

            var basicProperties = new Mock<IBasicProperties>();
            basicProperties.SetupGet(p => p.MessageId).Returns(jsonMessage.MessageId);
            basicProperties.SetupGet(p => p.AppId).Returns(jsonMessage.ApplicationId);
            basicProperties.SetupGet(p => p.ContentType).Returns(jsonMessage.ContentType);
            basicProperties.SetupGet(p => p.CorrelationId).Returns(jsonMessage.CorrelationId);
            basicProperties.SetupGet(p => p.Headers["CreationDateTime"]).Returns(Encoding.UTF8.GetBytes(creationTime.ToString("o", CultureInfo.InvariantCulture)));

            basicProperties.SetupGet(p => p.Type).Returns("topic");
            basicProperties.SetupGet(p => p.Timestamp).Returns(new AmqpTimestamp(creationTime.ToUnixTimeSeconds()));


            _model.Setup(p => p.QueueDeclare(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>()))
                .Returns(new QueueDeclareOk("queue-name", 1, 1));
            _model.Setup(p => p.QueueBind(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IDictionary<string, object>>()));
            _model.Setup(p => p.BasicQos(
                It.IsAny<uint>(),
                It.IsAny<ushort>(),
                It.IsAny<bool>()));
            _model.Setup(p => p.BasicConsume(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<IBasicConsumer>()))
                .Callback<string, bool, string, bool, bool, IDictionary<string, object>, IBasicConsumer>(
                    (queue, autoAck, tag, noLocal, exclusive, args, consumer) =>
                    {
                        consumer.HandleBasicDeliver(tag, Convert.ToUInt64(jsonMessage.DeliveryTag, CultureInfo.InvariantCulture), false, "exchange", "topic", basicProperties.Object, new ReadOnlyMemory<byte>(message.Body));
                    });

            var service = new RabbitMQMessageSubscriberService(_options, _logger.Object, _connectionFactory.Object);

            service.Subscribe("topic", "queue", (args) =>
            {
                Assert.Equal(message.ApplicationId, args.Message.ApplicationId);
                Assert.Equal(message.ContentType, args.Message.ContentType);
                Assert.Equal(message.MessageId, args.Message.MessageId);
                Assert.Equal(creationTime.ToUniversalTime(), args.Message.CreationDateTime.ToUniversalTime());
                Assert.Equal(message.DeliveryTag, args.Message.DeliveryTag);
                Assert.Equal("topic", args.Message.MessageDescription);
                Assert.Equal(message.MessageId, args.Message.MessageId);
                Assert.Equal(message.Body, args.Message.Body);

            });

            service.SubscribeAsync("topic", "queue", async (args) =>
            {
                await Task.Run(() =>
                {
                    s_messageReceived = args.Message;
                    service.Acknowledge(args.Message);
                }).ConfigureAwait(false);
            });

            // wait for it to pick up meassage
            Task.Delay(500).GetAwaiter().GetResult();

            Assert.NotNull(s_messageReceived);
            Assert.Equal(message.ApplicationId, s_messageReceived.ApplicationId);
            Assert.Equal(message.ContentType, s_messageReceived.ContentType);
            Assert.Equal(message.MessageId, s_messageReceived.MessageId);
            Assert.Equal(creationTime.ToUniversalTime(), s_messageReceived.CreationDateTime.ToUniversalTime());
            Assert.Equal(message.DeliveryTag, s_messageReceived.DeliveryTag);
            Assert.Equal("topic", s_messageReceived.MessageDescription);
            Assert.Equal(message.MessageId, s_messageReceived.MessageId);
            Assert.Equal(message.Body, s_messageReceived.Body);

        }

        [Fact(DisplayName = "Acknowledge a message")]
        public void AcknowledgeAMessage()
        {
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.EndPoint, "endpoint");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Username, "username");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Password, "password");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.VirtualHost, "virtual-host");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Exchange, "exchange");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.DeadLetterExchange, "exchange-dead-letter");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.DeliveryLimit, "3");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.RequeueDelay, "30");

            var jsonMessage = new JsonMessage<string>("hello world", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "1");
            var message = jsonMessage.ToMessage();

            _model.Setup(p => p.BasicAck(
                It.IsAny<ulong>(),
                It.IsAny<bool>()));

            var service = new RabbitMQMessageSubscriberService(_options, _logger.Object, _connectionFactory.Object);

            service.Acknowledge(message);

            _model.Verify(p => p.BasicAck(1, false), Times.Once());
        }

        [Fact(DisplayName = "Reject a message")]
        public void RejectAMessage()
        {
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.EndPoint, "endpoint");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Username, "username");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Password, "password");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.VirtualHost, "virtual-host");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Exchange, "exchange");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.DeadLetterExchange, "exchange-dead-letter");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.DeliveryLimit, "3");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.RequeueDelay, "30");

            var jsonMessage = new JsonMessage<string>("hello world", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "1");
            var message = jsonMessage.ToMessage();

            _model.Setup(p => p.BasicNack(
                It.IsAny<ulong>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()));

            var service = new RabbitMQMessageSubscriberService(_options, _logger.Object, _connectionFactory.Object);

            service.Reject(message);

            _model.Verify(p => p.BasicNack(1, false, true), Times.Once());
        }
    }
}
