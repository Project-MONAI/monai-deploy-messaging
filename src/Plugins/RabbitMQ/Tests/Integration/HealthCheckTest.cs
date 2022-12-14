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

using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Monai.Deploy.Messaging.Configuration;
using Xunit;

namespace Monai.Deploy.Messaging.RabbitMQ.Tests.Integration
{
    public class HealthCheckTest
    {
        public HealthCheckTest()
        { }

        [Fact]
        public async Task GivenAWebServer_WhenRabbitMqHealthChecksAreRegistered_ExpectToReturnInfoFromHealthEndpoint()
        {
            using var host = await new HostBuilder().ConfigureWebHost(SetupWebServer()).StartAsync().ConfigureAwait(false);

            var server = host.GetTestServer();
            server.BaseAddress = new Uri("https://example.com/");

            var client = server.CreateClient();
            var responseMessage = await client.GetAsync("health").ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
        }

        private static Action<IWebHostBuilder> SetupWebServer() => webBuilder =>
        {
            webBuilder
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions<MessageBrokerServiceConfiguration>().Configure(p =>
                    {
                        p.PublisherSettings[ConfigurationKeys.EndPoint] = RabbitMqConnection.HostName;
                        p.PublisherSettings[ConfigurationKeys.Username] = RabbitMqConnection.Username;
                        p.PublisherSettings[ConfigurationKeys.Password] = RabbitMqConnection.Password;
                        p.PublisherSettings[ConfigurationKeys.VirtualHost] = RabbitMqConnection.VirtualHost;
                        p.PublisherSettings[ConfigurationKeys.Exchange] = RabbitMqConnection.Exchange;
                        p.SubscriberSettings[ConfigurationKeys.EndPoint] = RabbitMqConnection.HostName;
                        p.SubscriberSettings[ConfigurationKeys.Username] = RabbitMqConnection.Username;
                        p.SubscriberSettings[ConfigurationKeys.Password] = RabbitMqConnection.Password;
                        p.SubscriberSettings[ConfigurationKeys.VirtualHost] = RabbitMqConnection.VirtualHost;
                        p.SubscriberSettings[ConfigurationKeys.Exchange] = RabbitMqConnection.Exchange;
                        p.SubscriberSettings[ConfigurationKeys.DeadLetterExchange] = RabbitMqConnection.DeadLetterExchange;
                        p.SubscriberSettings[ConfigurationKeys.DeliveryLimit] = RabbitMqConnection.DeliveryLimit;
                        p.SubscriberSettings[ConfigurationKeys.RequeueDelay] = RabbitMqConnection.RequeueDelay;
                    });
                    services.AddSingleton<IRabbitMQConnectionFactory, RabbitMQConnectionFactory>();
                    services.AddControllers();
                    var healtCheckBuilder = services.AddHealthChecks();
                    var subscriberHealthCheck = new SubscriberServicHealthCheckBuilder();
                    var publisherHealthCheck = new PublisherServicHealthCheckBuilder();
                    subscriberHealthCheck.Configure(healtCheckBuilder);
                    publisherHealthCheck.Configure(healtCheckBuilder);
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapHealthChecks("/health").AllowAnonymous();
                        endpoints.MapControllers();
                    });
                })
                .UseTestServer();
        };
    }
}
