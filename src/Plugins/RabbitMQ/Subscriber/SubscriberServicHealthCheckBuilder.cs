/*
 * Copyright 2021-2024 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Configuration;

namespace Monai.Deploy.Messaging.RabbitMQ
{
    public class SubscriberServicHealthCheckBuilder : SubscriberServiceHealthCheckRegistrationBase
    {
        public override IHealthChecksBuilder Configure(
            IHealthChecksBuilder builder,
            HealthStatus? failureStatus = null,
            IEnumerable<string>? tags = null,
            TimeSpan? timeout = null)
        {
            builder.Add(new HealthCheckRegistration(
                ConfigurationKeys.SubscriberServiceName,
                serviceProvider =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<MessageBrokerServiceConfiguration>>();
                    var logger = serviceProvider.GetRequiredService<ILogger<RabbitMQHealthCheck>>();
                    var connectionFactory = serviceProvider.GetRequiredService<IRabbitMQConnectionFactory>();
                    return new RabbitMQHealthCheck(connectionFactory, options.Value.SubscriberSettings, logger, RabbitMQMessageSubscriberService.ValidateConfiguration);
                },
                failureStatus,
                tags,
                timeout));
            return builder;
        }
    }
}
