// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Messaging.Messages;
using Monai.Deploy.Messaging.RabbitMq;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace Monai.Deploy.Messaging.Test.RabbitMq
{
    public class RabbitMqMessageSubscriberServiceTest
    {
        private readonly IOptions<MessageBrokerServiceConfiguration> _options;
        private readonly Mock<ILogger<RabbitMqMessageSubscriberService>> _logger;
        private readonly Mock<IRabbitMqConnectionFactory> _connectionFactory;
        private readonly Mock<IConnection> _connection;
        private readonly Mock<IModel> _model;

        public RabbitMqMessageSubscriberServiceTest()
        {
            _options = Options.Create(new MessageBrokerServiceConfiguration());
            _logger = new Mock<ILogger<RabbitMqMessageSubscriberService>>();
            _connectionFactory = new Mock<IRabbitMqConnectionFactory>();
            _connection = new Mock<IConnection>();
            _model = new Mock<IModel>();

            _connectionFactory.Setup(p => p.CreateConnection(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_connection.Object);

            _connection.Setup(p => p.CreateModel()).Returns(_model.Object);
        }

        [Fact(DisplayName = "Fails to validate when required keys are missing")]
        public void FailsToValidateWhenRequiredKeysAreMissing()
        {
            Assert.Throws<ConfigurationException>(() => new RabbitMqMessageSubscriberService(_options, _logger.Object, _connectionFactory.Object));
        }

        [Fact(DisplayName = "Cleanup connections on Dispose")]
        public void CleanupOnDispose()
        {
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.EndPoint, "endpoint");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Username, "username");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Password, "password");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.VirtualHost, "virtual-host");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Exchange, "exchange");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.ExportRequestQueue, "export-request-queue");

            var service = new RabbitMqMessageSubscriberService(_options, _logger.Object, _connectionFactory.Object);
            service.Dispose();

            _connection.Verify(p => p.Close(), Times.Once());
            _connection.Verify(p => p.Dispose(), Times.Once());
        }

        [Fact(DisplayName = "Subscribes to a topic")]
        public async Task SubscribesToATopic()
        {
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.EndPoint, "endpoint");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Username, "username");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Password, "password");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.VirtualHost, "virtual-host");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Exchange, "exchange");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.ExportRequestQueue, "export-request-queue");

            var jsonMessage = new JsonMessage<string>("hello world", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "1");
            var message = jsonMessage.ToMessage();
            var basicProperties = new Mock<IBasicProperties>();
            basicProperties.SetupGet(p => p.MessageId).Returns(jsonMessage.MessageId);
            basicProperties.SetupGet(p => p.AppId).Returns(jsonMessage.ApplicationId);
            basicProperties.SetupGet(p => p.ContentType).Returns(jsonMessage.ContentType);
            basicProperties.SetupGet(p => p.CorrelationId).Returns(jsonMessage.CorrelationId);
            basicProperties.SetupGet(p => p.Headers["CreationDateTime"]).Returns(Encoding.UTF8.GetBytes(jsonMessage.CreationDateTime.ToString("o", System.Globalization.CultureInfo.InvariantCulture)));

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
                        consumer.HandleBasicDeliver(tag, Convert.ToUInt64(jsonMessage.DeliveryTag), false, "exchange", "topic", basicProperties.Object, new ReadOnlyMemory<byte>(message.Body));
                    });

            var service = new RabbitMqMessageSubscriberService(_options, _logger.Object, _connectionFactory.Object);

            service.Subscribe("topic", "queue", (args) =>
            {
                Assert.Equal(message.ApplicationId, args.Message.ApplicationId);
                Assert.Equal(message.ContentType, args.Message.ContentType);
                Assert.Equal(message.MessageId, args.Message.MessageId);
                Assert.Equal(message.CreationDateTime.ToUniversalTime(), args.Message.CreationDateTime.ToUniversalTime());
                Assert.Equal(message.DeliveryTag, args.Message.DeliveryTag);
                Assert.Equal("topic", args.Message.MessageDescription);
                Assert.Equal(message.MessageId, args.Message.MessageId);
                Assert.Equal(message.Body, args.Message.Body);
            });
        }

        [Fact(DisplayName = "Acknowledge a message")]
        public void AcknowledgeAMessage()
        {
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.EndPoint, "endpoint");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Username, "username");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Password, "password");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.VirtualHost, "virtual-host");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.Exchange, "exchange");
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.ExportRequestQueue, "export-request-queue");

            var jsonMessage = new JsonMessage<string>("hello world", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "1");
            var message = jsonMessage.ToMessage();

            _model.Setup(p => p.BasicAck(
                It.IsAny<ulong>(),
                It.IsAny<bool>()));

            var service = new RabbitMqMessageSubscriberService(_options, _logger.Object, _connectionFactory.Object);

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
            _options.Value.SubscriberSettings.Add(ConfigurationKeys.ExportRequestQueue, "export-request-queue");

            var jsonMessage = new JsonMessage<string>("hello world", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "1");
            var message = jsonMessage.ToMessage();

            _model.Setup(p => p.BasicNack(
                It.IsAny<ulong>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()));

            var service = new RabbitMqMessageSubscriberService(_options, _logger.Object, _connectionFactory.Object);

            service.Reject(message);

            _model.Verify(p => p.BasicNack(1, false, true), Times.Once());
        }
    }
}
