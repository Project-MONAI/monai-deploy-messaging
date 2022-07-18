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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Messaging.Messages;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace Monai.Deploy.Messaging.RabbitMQ.Tests
{
    public class RabbitMQMessagePublisherServiceTest
    {
        private readonly IOptions<MessageBrokerServiceConfiguration> _options;
        private readonly Mock<ILogger<RabbitMQMessagePublisherService>> _logger;
        private readonly Mock<IRabbitMQConnectionFactory> _connectionFactory;
        private readonly Mock<IModel> _model;

        public RabbitMQMessagePublisherServiceTest()
        {
            _options = Options.Create(new MessageBrokerServiceConfiguration());
            _logger = new Mock<ILogger<RabbitMQMessagePublisherService>>();
            _connectionFactory = new Mock<IRabbitMQConnectionFactory>();
            _model = new Mock<IModel>();

            _connectionFactory.Setup(p => p.CreateChannel(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_model.Object);
        }

        [Fact(DisplayName = "Fails to validate when required keys are missing")]
        public void FailsToValidateWhenRequiredKeysAreMissing()
        {
            Assert.Throws<ConfigurationException>(() => new RabbitMQMessagePublisherService(_options, _logger.Object, _connectionFactory.Object));
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

            var service = new RabbitMQMessagePublisherService(_options, _logger.Object, _connectionFactory.Object);

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

            _model.Verify(p => p.Dispose(), Times.Once());
        }
    }
}
