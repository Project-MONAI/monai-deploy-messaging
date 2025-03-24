/*
 * Copyright 2021-2025 MONAI Consortium
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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Monai.Deploy.Messaging.RabbitMQ
{
    internal class RabbitMQHealthCheck : IHealthCheck
    {
        private readonly IRabbitMQConnectionFactory _connectionFactory;
        private readonly Dictionary<string, string> _options;
        private readonly ILogger<RabbitMQHealthCheck> _logger;

        public RabbitMQHealthCheck(
            IRabbitMQConnectionFactory connectionFactory,
            Dictionary<string, string> options,
            ILogger<RabbitMQHealthCheck> logger,
            Action<Dictionary<string, string>> validator)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            validator(options);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
        {
            try
            {
                var channel = _connectionFactory.CreateChannel(
                    ChannelType.Subscriber,
                    _options[ConfigurationKeys.EndPoint],
                    _options[ConfigurationKeys.Username],
                    _options[ConfigurationKeys.Password],
                    _options[ConfigurationKeys.VirtualHost],
                    _options.ContainsKey(ConfigurationKeys.UseSSL) ? _options[ConfigurationKeys.UseSSL] : string.Empty,
                    _options.ContainsKey(ConfigurationKeys.Port) ? _options[ConfigurationKeys.Port] : string.Empty);

                return await Task.FromResult(HealthCheckResult.Healthy()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.HealthCheckError(ex);
                return await Task.FromResult(HealthCheckResult.Unhealthy(exception: ex)).ConfigureAwait(false);
            }
        }
    }
}
