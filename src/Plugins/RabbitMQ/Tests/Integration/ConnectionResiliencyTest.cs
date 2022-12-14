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
using Moq;
using Xunit;

namespace Monai.Deploy.Messaging.RabbitMQ.Tests.Integration
{
    public class ConnectionResiliencyTest
    {
        private readonly Mock<ILogger<RabbitMQConnectionFactory>> _logger;
        private readonly RabbitMQConnectionFactory _factory;

        public ConnectionResiliencyTest()
        {
            _logger = new Mock<ILogger<RabbitMQConnectionFactory>>();
            _factory = new RabbitMQConnectionFactory(_logger.Object);
        }

        [Fact]
        public void GivenAModel_OnClose_ShouldBeAbleToRecreate()
        {
            var manualResetEvent = new ManualResetEventSlim();

            var channel = _factory.CreateChannel(ChannelType.Publisher, RabbitMqConnection.HostName, RabbitMqConnection.Username, RabbitMqConnection.Password, RabbitMqConnection.VirtualHost, RabbitMqConnection.UseSsl, RabbitMqConnection.PortNumber);
            channel.ModelShutdown += (sender, e) =>
            {
                manualResetEvent.Set();
            };
            channel.Close();

            manualResetEvent.Wait();

            var exception = Record.Exception(() =>
            {
                var newChannel = _factory.CreateChannel(ChannelType.Publisher, RabbitMqConnection.HostName, RabbitMqConnection.Username, RabbitMqConnection.Password, RabbitMqConnection.VirtualHost, RabbitMqConnection.UseSsl, RabbitMqConnection.PortNumber);
                Assert.NotNull(newChannel);
            });

            Assert.Null(exception);
        }

        [Fact]
        public void GivenAModel_OnAbort_ShouldBeAbleToRecreate()
        {
            var manualResetEvent = new ManualResetEventSlim();

            var channel = _factory.CreateChannel(ChannelType.Publisher, RabbitMqConnection.HostName, RabbitMqConnection.Username, RabbitMqConnection.Password, RabbitMqConnection.VirtualHost, RabbitMqConnection.UseSsl, RabbitMqConnection.PortNumber);
            channel.ModelShutdown += (sender, e) =>
            {
                manualResetEvent.Set();
            };
            channel.Abort();

            manualResetEvent.Wait();

            var exception = Record.Exception(() =>
            {
                var newChannel = _factory.CreateChannel(ChannelType.Publisher, RabbitMqConnection.HostName, RabbitMqConnection.Username, RabbitMqConnection.Password, RabbitMqConnection.VirtualHost, RabbitMqConnection.UseSsl, RabbitMqConnection.PortNumber);
                Assert.NotNull(newChannel);
            });

            Assert.Null(exception);
        }

        [Fact]
        public void GivenAModel_OnDispose_ShouldBeAbleToRecreate()
        {
            var manualResetEvent = new ManualResetEventSlim();

            var channel = _factory.CreateChannel(ChannelType.Publisher, RabbitMqConnection.HostName, RabbitMqConnection.Username, RabbitMqConnection.Password, RabbitMqConnection.VirtualHost, RabbitMqConnection.UseSsl, RabbitMqConnection.PortNumber);
            channel.ModelShutdown += (sender, e) =>
            {
                manualResetEvent.Set();
            };
            channel.Dispose();

            manualResetEvent.Wait();

            var exception = Record.Exception(() =>
            {
                var newChannel = _factory.CreateChannel(ChannelType.Publisher, RabbitMqConnection.HostName, RabbitMqConnection.Username, RabbitMqConnection.Password, RabbitMqConnection.VirtualHost, RabbitMqConnection.UseSsl, RabbitMqConnection.PortNumber);
                Assert.NotNull(newChannel);
            });

            Assert.Null(exception);
        }

        [Fact]
        public void GivenAConnection_OnClose_ShouldBeAbleToRecreate()
        {
            var manualResetEvent = new ManualResetEventSlim();

            var channel = _factory.CreateChannel(ChannelType.Publisher, RabbitMqConnection.HostName, RabbitMqConnection.Username, RabbitMqConnection.Password, RabbitMqConnection.VirtualHost, RabbitMqConnection.UseSsl, RabbitMqConnection.PortNumber);

            Assert.Single(_factory.Connections);

            _factory.Connections[0].ConnectionShutdown += (sender, e) =>
            {
                manualResetEvent.Set();
            };
            _factory.Connections[0].Close();
            manualResetEvent.Wait();

            var exception = Record.Exception(() =>
            {
                var newChannel = _factory.CreateChannel(ChannelType.Publisher, RabbitMqConnection.HostName, RabbitMqConnection.Username, RabbitMqConnection.Password, RabbitMqConnection.VirtualHost, RabbitMqConnection.UseSsl, RabbitMqConnection.PortNumber);
                Assert.NotNull(newChannel);
            });

            Assert.Null(exception);
        }

        [Fact]
        public void GivenAConnection_OnAbort_ShouldBeAbleToRecreate()
        {
            var manualResetEvent = new ManualResetEventSlim();

            var channel = _factory.CreateChannel(ChannelType.Publisher, RabbitMqConnection.HostName, RabbitMqConnection.Username, RabbitMqConnection.Password, RabbitMqConnection.VirtualHost, RabbitMqConnection.UseSsl, RabbitMqConnection.PortNumber);

            Assert.Single(_factory.Connections);

            _factory.Connections[0].ConnectionShutdown += (sender, e) =>
            {
                manualResetEvent.Set();
            };
            _factory.Connections[0].Abort();
            manualResetEvent.Wait();

            var exception = Record.Exception(() =>
            {
                var newChannel = _factory.CreateChannel(ChannelType.Publisher, RabbitMqConnection.HostName, RabbitMqConnection.Username, RabbitMqConnection.Password, RabbitMqConnection.VirtualHost, RabbitMqConnection.UseSsl, RabbitMqConnection.PortNumber);
                Assert.NotNull(newChannel);
            });

            Assert.Null(exception);
        }

        [Fact]
        public void GivenAConnection_OnDispose_ShouldBeAbleToRecreate()
        {
            var manualResetEvent = new ManualResetEventSlim();

            var channel = _factory.CreateChannel(ChannelType.Publisher, RabbitMqConnection.HostName, RabbitMqConnection.Username, RabbitMqConnection.Password, RabbitMqConnection.VirtualHost, RabbitMqConnection.UseSsl, RabbitMqConnection.PortNumber);

            Assert.Single(_factory.Connections);

            _factory.Connections[0].ConnectionShutdown += (sender, e) =>
            {
                manualResetEvent.Set();
            };
            _factory.Connections[0].Dispose();
            manualResetEvent.Wait();

            var exception = Record.Exception(() =>
            {
                var newChannel = _factory.CreateChannel(ChannelType.Publisher, RabbitMqConnection.HostName, RabbitMqConnection.Username, RabbitMqConnection.Password, RabbitMqConnection.VirtualHost, RabbitMqConnection.UseSsl, RabbitMqConnection.PortNumber);
                Assert.NotNull(newChannel);
            });

            Assert.Null(exception);
        }
    }
}
