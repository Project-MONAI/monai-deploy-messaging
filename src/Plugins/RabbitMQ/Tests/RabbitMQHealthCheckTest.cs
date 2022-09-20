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

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace Monai.Deploy.Messaging.RabbitMQ.Tests
{
    public class RabbitMQHealthCheckTest
    {
        private readonly Mock<IRabbitMQConnectionFactory> _connectionFactory;
        private readonly Mock<ILogger<RabbitMQHealthCheck>> _logger;
        private readonly Dictionary<string, string> _options;

        public RabbitMQHealthCheckTest()
        {
            _connectionFactory = new Mock<IRabbitMQConnectionFactory>();
            _logger = new Mock<ILogger<RabbitMQHealthCheck>>();
            _options = new Dictionary<string, string>();
            _options.Add(ConfigurationKeys.EndPoint, ConfigurationKeys.EndPoint);
            _options.Add(ConfigurationKeys.Username, ConfigurationKeys.Username);
            _options.Add(ConfigurationKeys.Password, ConfigurationKeys.Password);
            _options.Add(ConfigurationKeys.VirtualHost, ConfigurationKeys.VirtualHost);
        }

        [Fact]
        public async Task CheckHealthAsync_WhenFailedToListBucket_ReturnUnhealthy()
        {
            _connectionFactory.Setup(p => p.CreateChannel(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("error"));

            var healthCheck = new RabbitMQHealthCheck(_connectionFactory.Object, _options, _logger.Object, (d) => { });
            var results = await healthCheck.CheckHealthAsync(new HealthCheckContext()).ConfigureAwait(false);

            Assert.Equal(HealthStatus.Unhealthy, results.Status);
            Assert.NotNull(results.Exception);
            Assert.Equal("error", results.Exception.Message);
        }

        [Fact]
        public async Task CheckHealthAsync_WhenListBucketSucceeds_ReturnHealthy()
        {
            var channel = new Mock<IModel>();
            channel.Setup(p => p.Close());
            _connectionFactory.Setup(p => p.CreateChannel(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(channel.Object);
            var healthCheck = new RabbitMQHealthCheck(_connectionFactory.Object, _options, _logger.Object, (d) => { });
            var results = await healthCheck.CheckHealthAsync(new HealthCheckContext()).ConfigureAwait(false);

            Assert.Equal(HealthStatus.Healthy, results.Status);
            Assert.Null(results.Exception);

            channel.Verify(p => p.Close(), Times.Once());
        }
    }
}
