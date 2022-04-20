// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
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
    public class RabbitMqMessagePublisherServiceTest
    {
        private readonly IOptions<MessageBrokerServiceConfiguration> _options;
        private readonly Mock<ILogger<RabbitMqMessagePublisherService>> _logger;
        private readonly Mock<IRabbitMqConnectionFactory> _connectionFactory;
        private readonly Mock<IConnection> _connection;
        private readonly Mock<IModel> _model;

        public RabbitMqMessagePublisherServiceTest()
        {
            _options = Options.Create(new MessageBrokerServiceConfiguration());
            _logger = new Mock<ILogger<RabbitMqMessagePublisherService>>();
            _connectionFactory = new Mock<IRabbitMqConnectionFactory>();
            _connection = new Mock<IConnection>();
            _model = new Mock<IModel>();

            _connectionFactory.Setup(p => p.CreateConnection(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(_connection.Object);

            _connection.Setup(p => p.CreateModel()).Returns(_model.Object);
        }

        [Fact(DisplayName = "Fails to validate when required keys are missing")]
        public void FailsToValidateWhenRequiredKeysAreMissing()
        {
            Assert.Throws<ConfigurationException>(() => new RabbitMqMessagePublisherService(_options, _logger.Object, _connectionFactory.Object));
        }

        [Fact(DisplayName = "Cleanup connections on Dispose")]
        public void CleanupOnDispose()
        {
            _options.Value.PublisherSettings.Add(ConfigurationKeys.EndPoint, "endpoint");
            _options.Value.PublisherSettings.Add(ConfigurationKeys.Username, "username");
            _options.Value.PublisherSettings.Add(ConfigurationKeys.Password, "password");
            _options.Value.PublisherSettings.Add(ConfigurationKeys.VirtualHost, "virtual-host");
            _options.Value.PublisherSettings.Add(ConfigurationKeys.Exchange, "exchange");

            var service = new RabbitMqMessagePublisherService(_options, _logger.Object, _connectionFactory.Object);
            service.Dispose();

            _connection.Verify(p => p.Close(), Times.Once());
            _connection.Verify(p => p.Dispose(), Times.Once());

        }

        [Fact(DisplayName = "Publishes a message")]
        public async Task PublishesAMessage()
        {
            _options.Value.PublisherSettings.Add(ConfigurationKeys.EndPoint, "endpoint");
            _options.Value.PublisherSettings.Add(ConfigurationKeys.Username, "username");
            _options.Value.PublisherSettings.Add(ConfigurationKeys.Password, "password");
            _options.Value.PublisherSettings.Add(ConfigurationKeys.VirtualHost, "virtual-host");
            _options.Value.PublisherSettings.Add(ConfigurationKeys.Exchange, "exchange");

            var basicProperties = new Mock<IBasicProperties>();
            _model.Setup(p => p.CreateBasicProperties()).Returns(basicProperties.Object);
            _model.Setup(p => p.BasicPublish(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()));

            var service = new RabbitMqMessagePublisherService(_options, _logger.Object, _connectionFactory.Object);

            var jsonMessage = new JsonMessage<string>("hello world", Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var message = jsonMessage.ToMessage();
            await service.Publish("topic", message).ConfigureAwait(false);

            basicProperties.VerifySet(p => p.Persistent = true);
            basicProperties.VerifySet(p => p.ContentType = jsonMessage.ContentType);
            basicProperties.VerifySet(p => p.MessageId = jsonMessage.MessageId);
            basicProperties.VerifySet(p => p.AppId = jsonMessage.ApplicationId);
            basicProperties.VerifySet(p => p.CorrelationId = jsonMessage.CorrelationId);
            basicProperties.VerifySet(p => p.DeliveryMode = 2);

            _model.Verify(p => p.BasicPublish(
                It.Is<string>(p => p.Equals("exchange")),
                It.Is<string>(p => p.Equals("topic")),
                false,
                It.Is<IBasicProperties>(p => p.Equals(basicProperties.Object)),
                It.IsAny<ReadOnlyMemory<byte>>()), Times.Once());
        }
    }
}
