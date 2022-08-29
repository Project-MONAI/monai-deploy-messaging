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

using System.IO.Abstractions;
using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Configuration;

namespace Monai.Deploy.Messaging
{
    public static class IHealthChecksBuilderExtensions
    {
        /// <summary>
        /// Configures health check for the MONAI Deploy Message Broker Subscriber Service.
        /// </summary>
        /// <param name="builder">Instance of <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="serviceCollection">Instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="serviceProvider">Instance of <see cref="IServiceProvider"/>.</param>
        /// <param name="fullyQualifiedTypeName">Fully qualified type name of the service to use.</param>
        /// <returns>Instance of <see cref="IHealthChecksBuilder"/>.</returns>
        /// <exception cref="ConfigurationException"></exception>
        public static IHealthChecksBuilder AddMonaiDeployMessageBrokerSubscriberHealthCheck(
            this IHealthChecksBuilder builder,
            IServiceCollection serviceCollection,
            string fullyQualifiedTypeName,
            HealthStatus? failureStatus = null,
            IEnumerable<string>? tags = null,
            TimeSpan? timeout = null)
            => AddMonaiDeployMessageBrokerSubscriberHealthCheck(builder, serviceCollection, fullyQualifiedTypeName, new FileSystem(), failureStatus, tags, timeout);

        /// <summary>
        /// Configures health check for the MONAI Deploy Message Broker Subscriber Service.
        /// </summary>
        /// <param name="builder">Instance of <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="serviceCollection">Instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="fullyQualifiedTypeName">Fully qualified type name of the service to use.</param>
        /// <param name="fileSystem">Instance of <see cref="IFileSystem"/>.</param>
        /// <returns>Instance of <see cref="IHealthChecksBuilder"/>.</returns>
        /// <exception cref="ConfigurationException"></exception>
        public static IHealthChecksBuilder AddMonaiDeployMessageBrokerSubscriberHealthCheck(
            this IHealthChecksBuilder builder,
            IServiceCollection serviceCollection,
            string fullyQualifiedTypeName,
            IFileSystem fileSystem,
            HealthStatus? failureStatus = null,
            IEnumerable<string>? tags = null,
            TimeSpan? timeout = null)
            => Add<IMessageBrokerSubscriberService, SubscriberServiceRegistrationBase, SubscriberServiceHealthCheckRegistrationBase>(builder, serviceCollection, fullyQualifiedTypeName, fileSystem, failureStatus, tags, timeout);

        /// <summary>
        /// Configures health check for the MONAI Deploy Message Broker Publisher Service.
        /// </summary>
        /// <param name="builder">Instance of <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="serviceCollection">Instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="serviceProvider">Instance of <see cref="IServiceProvider"/>.</param>
        /// <param name="fullyQualifiedTypeName">Fully qualified type name of the service to use.</param>
        /// <returns>Instance of <see cref="IHealthChecksBuilder"/>.</returns>
        /// <exception cref="ConfigurationException"></exception>
        public static IHealthChecksBuilder AddMonaiDeployMessageBrokerPublisherHealthCheck(
            this IHealthChecksBuilder builder,
            IServiceCollection serviceCollection,
            string fullyQualifiedTypeName,
            HealthStatus? failureStatus = null,
            IEnumerable<string>? tags = null,
            TimeSpan? timeout = null)
            => AddMonaiDeployMessageBrokerPublisherHealthCheck(builder, serviceCollection, fullyQualifiedTypeName, new FileSystem(), failureStatus, tags, timeout);

        /// <summary>
        /// Configures health check for the MONAI Deploy Message Broker Publisher Service.
        /// </summary>
        /// <param name="builder">Instance of <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="serviceCollection">Instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="serviceProvider">Instance of <see cref="IServiceProvider"/>.</param>
        /// <param name="fullyQualifiedTypeName">Fully qualified type name of the service to use.</param>
        /// <param name="fileSystem">Instance of <see cref="IFileSystem"/>.</param>
        /// <returns>Instance of <see cref="IHealthChecksBuilder"/>.</returns>
        /// <exception cref="ConfigurationException"></exception>
        public static IHealthChecksBuilder AddMonaiDeployMessageBrokerPublisherHealthCheck(
            this IHealthChecksBuilder builder,
            IServiceCollection serviceCollection,
            string fullyQualifiedTypeName,
            IFileSystem fileSystem,
            HealthStatus? failureStatus = null,
            IEnumerable<string>? tags = null,
            TimeSpan? timeout = null)
            => Add<IMessageBrokerPublisherService, PublisherServiceRegistrationBase, PublisherServiceHealthCheckRegistrationBase>(builder, serviceCollection, fullyQualifiedTypeName, fileSystem, failureStatus, tags, timeout);

        private static IHealthChecksBuilder Add<T, U, V>(
            this IHealthChecksBuilder builder,
            IServiceCollection serviceCollection,
            string fullyQualifiedTypeName,
            IFileSystem fileSystem,
            HealthStatus? failureStatus = null,
            IEnumerable<string>? tags = null,
            TimeSpan? timeout = null)
                where U : ServiceRegistrationBase
                where V : HealthCheckRegistrationBase
        {
            Guard.Against.NullOrWhiteSpace(fullyQualifiedTypeName, nameof(fullyQualifiedTypeName));
            Guard.Against.Null(fileSystem, nameof(fileSystem));

            ResolveEventHandler resolveEventHandler = (sender, args) =>
            {
                return IServiceCollectionExtensions.CurrentDomain_AssemblyResolve(args, fileSystem);
            };

            AppDomain.CurrentDomain.AssemblyResolve += resolveEventHandler;

            try
            {
                var serviceAssembly = IServiceCollectionExtensions.LoadAssemblyFromDisk(IServiceCollectionExtensions.GetAssemblyName(fullyQualifiedTypeName), fileSystem);
                var serviceRegistrationType = serviceAssembly.GetTypes().FirstOrDefault(p => p.BaseType == typeof(U));

                if (serviceRegistrationType is null || Activator.CreateInstance(serviceRegistrationType, fullyQualifiedTypeName) is not U serviceRegistrar)
                {
                    throw new ConfigurationException($"Service registrar cannot be found for the configured plug-in '{fullyQualifiedTypeName}'.");
                }

                var healthCheckBaseType = serviceAssembly.GetTypes().FirstOrDefault(p => p.BaseType == typeof(V));

                if (healthCheckBaseType is null || Activator.CreateInstance(healthCheckBaseType, fullyQualifiedTypeName) is not V healthCheckBuilder)
                {
                    throw new ConfigurationException($"Health check registrar cannot be found for the configured plug-in '{fullyQualifiedTypeName}'.");
                }

                if (!IServiceCollectionExtensions.IsSupportedType<T>(fullyQualifiedTypeName, serviceAssembly))
                {
                    throw new ConfigurationException($"The configured type '{fullyQualifiedTypeName}' does not implement the {typeof(T).Name} interface.");
                }

                serviceRegistrar.Configure(serviceCollection);

                healthCheckBuilder.Configure(builder, failureStatus, tags, timeout);

                return builder;
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= resolveEventHandler;
            }
        }
    }
}
