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

using System.IO.Abstractions;
using System.Reflection;
using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Configuration;

namespace Monai.Deploy.Messaging
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Configures all dependencies required for the MONAI Deploy Message Broker Subscriber Service.
        /// </summary>
        /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="fullyQualifiedTypeName">Fully qualified type name of the service to use.</param>
        /// <param name="registerHealthCheck">bool as to if the healthcheck is be registered</param>
        /// <param name="failureStatus"></param>
        /// <param name="tags"></param>
        /// <param name="timeout"></param>
        /// <returns>Instance of <see cref="IServiceCollection"/>.</returns>
        /// <exception cref="ConfigurationException"></exception>
        public static IServiceCollection AddMonaiDeployMessageBrokerSubscriberService(
            this IServiceCollection services,
            string fullyQualifiedTypeName,
            bool registerHealthCheck = true,
            HealthStatus? failureStatus = null,
            IEnumerable<string>? tags = null,
            TimeSpan? timeout = null)
            => AddMonaiDeployMessageBrokerSubscriberService(services, fullyQualifiedTypeName, new FileSystem(), registerHealthCheck, failureStatus, tags, timeout);

        /// <summary>
        /// Configures all dependencies required for the MONAI Deploy Message Broker Subscriber Service.
        /// </summary>
        /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="fullyQualifiedTypeName">Fully qualified type name of the service to use.</param>
        /// <param name="fileSystem">Instance of <see cref="IFileSystem"/>.</param>
        /// <param name="registerHealthCheck">bool as to if the healthcheck is be registered</param>
        /// <param name="failureStatus"></param>
        /// <param name="tags"></param>
        /// <param name="timeout"></param>
        /// <returns>Instance of <see cref="IServiceCollection"/>.</returns>
        /// <exception cref="ConfigurationException"></exception>
        public static IServiceCollection AddMonaiDeployMessageBrokerSubscriberService(
            this IServiceCollection services,
            string fullyQualifiedTypeName,
            IFileSystem fileSystem,
            bool registerHealthCheck = true,
            HealthStatus? failureStatus = null,
            IEnumerable<string>? tags = null,
            TimeSpan? timeout = null)
            => Add<IMessageBrokerSubscriberService, SubscriberServiceRegistrationBase, SubscriberServiceHealthCheckRegistrationBase>(services, fullyQualifiedTypeName, fileSystem, registerHealthCheck, failureStatus, tags, timeout);

        /// <summary>
        /// Configures all dependencies required for the MONAI Deploy Message Broker Publisher Service.
        /// </summary>
        /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="fullyQualifiedTypeName">Fully qualified type name of the service to use.</param>
        /// <param name="registerHealthCheck">bool as to if the healthcheck is be registered</param>
        /// <param name="failureStatus"></param>
        /// <param name="tags"></param>
        /// <param name="timeout"></param>
        /// <returns>Instance of <see cref="IServiceCollection"/>.</returns>
        /// <exception cref="ConfigurationException"></exception>
        public static IServiceCollection AddMonaiDeployMessageBrokerPublisherService(
            this IServiceCollection services,
            string fullyQualifiedTypeName,
            bool registerHealthCheck = true,
            HealthStatus? failureStatus = null,
            IEnumerable<string>? tags = null,
            TimeSpan? timeout = null)
            => AddMonaiDeployMessageBrokerPublisherService(services, fullyQualifiedTypeName, new FileSystem(), registerHealthCheck, failureStatus, tags, timeout);

        /// <summary>
        /// Configures all dependencies required for the MONAI Deploy Message Broker Publisher Service.
        /// </summary>
        /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="fullyQualifiedTypeName">Fully qualified type name of the service to use.</param>
        /// <param name="fileSystem">Instance of <see cref="IFileSystem"/>.</param>
        /// <param name="registerHealthCheck">bool as to if the healthcheck is be registered</param>
        /// <param name="failureStatus"></param>
        /// <param name="tags"></param>
        /// <param name="timeout"></param>
        /// <returns>Instance of <see cref="IServiceCollection"/>.</returns>
        /// <exception cref="ConfigurationException"></exception>
        public static IServiceCollection AddMonaiDeployMessageBrokerPublisherService(
            this IServiceCollection services,
            string fullyQualifiedTypeName,
            IFileSystem fileSystem,
            bool registerHealthCheck = true,
            HealthStatus? failureStatus = null,
            IEnumerable<string>? tags = null,
            TimeSpan? timeout = null)
            => Add<IMessageBrokerPublisherService, PublisherServiceRegistrationBase, PublisherServiceHealthCheckRegistrationBase>(services, fullyQualifiedTypeName, fileSystem, registerHealthCheck, failureStatus, tags, timeout);

        private static IServiceCollection Add<T, U, V>(
            this IServiceCollection services,
            string fullyQualifiedTypeName,
            IFileSystem fileSystem,
            bool registerHealthCheck = true,
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
                return CurrentDomain_AssemblyResolve(args, fileSystem);
            };

            AppDomain.CurrentDomain.AssemblyResolve += resolveEventHandler;

            try
            {
                var serviceAssembly = LoadAssemblyFromDisk(GetAssemblyName(fullyQualifiedTypeName), fileSystem);

                if (!IsSupportedType<T>(fullyQualifiedTypeName, serviceAssembly))
                {
                    throw new ConfigurationException($"The configured type '{fullyQualifiedTypeName}' does not implement the {typeof(T).Name} interface.");
                }

                RegisterServices<U>(services, fullyQualifiedTypeName, serviceAssembly);

                if (registerHealthCheck)
                {
                    RegisterHealtChecks<V>(services, fullyQualifiedTypeName, serviceAssembly, failureStatus, tags, timeout);
                }

                return services;
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= resolveEventHandler;
            }
        }

        private static void RegisterHealtChecks<V>(
            IServiceCollection services,
            string fullyQualifiedTypeName,
            Assembly serviceAssembly,
            HealthStatus? failureStatus,
            IEnumerable<string>? tags,
            TimeSpan? timeout) where V : HealthCheckRegistrationBase
        {
            var healthCheckBaseType = serviceAssembly.GetTypes().FirstOrDefault(p => p.BaseType == typeof(V));

            if (healthCheckBaseType is null || Activator.CreateInstance(healthCheckBaseType) is not V healthCheckBuilderBase)
            {
                throw new ConfigurationException($"Health check registrar cannot be found for the configured plug-in '{fullyQualifiedTypeName}'.");
            }

            var healthCheckBuilder = services.AddHealthChecks();
            healthCheckBuilderBase.Configure(healthCheckBuilder, failureStatus, tags, timeout);
        }

        private static void RegisterServices<U>(
            IServiceCollection services,
            string fullyQualifiedTypeName,
            Assembly serviceAssembly) where U : ServiceRegistrationBase
        {
            var serviceRegistrationType = serviceAssembly.GetTypes().FirstOrDefault(p => p.BaseType == typeof(U));

            if (serviceRegistrationType is null || Activator.CreateInstance(serviceRegistrationType) is not U serviceRegistrar)
            {
                throw new ConfigurationException($"Service registrar cannot be found for the configured plug-in '{fullyQualifiedTypeName}'.");
            }

            serviceRegistrar.Configure(services);
        }

        internal static bool IsSupportedType<T>(string fullyQualifiedTypeName, Assembly storageServiceAssembly)
        {
            Guard.Against.NullOrWhiteSpace(fullyQualifiedTypeName, nameof(fullyQualifiedTypeName));
            Guard.Against.Null(storageServiceAssembly, nameof(storageServiceAssembly));

            var storageServiceType = Type.GetType(fullyQualifiedTypeName, assemblyeName => storageServiceAssembly, null, false);

            return storageServiceType is not null &&
                storageServiceType.GetInterfaces().Contains(typeof(T));
        }

        internal static string GetAssemblyName(string fullyQualifiedTypeName)
        {
            var assemblyNameParts = fullyQualifiedTypeName.Split(',', StringSplitOptions.None);
            if (assemblyNameParts.Length < 2 || string.IsNullOrWhiteSpace(assemblyNameParts[1]))
            {
                throw new ConfigurationException($"The configured service type '{fullyQualifiedTypeName}' is not a valid fully qualified type name.  E.g. {MessageBrokerServiceConfiguration.DefaultPublisherAssemblyName}")
                {
                    HelpLink = "https://docs.microsoft.com/en-us/dotnet/standard/assembly/find-fully-qualified-name"
                };
            }

            return assemblyNameParts[1].Trim();
        }

        internal static Assembly CurrentDomain_AssemblyResolve(ResolveEventArgs args, IFileSystem fileSystem)
        {
            Guard.Against.Null(args, nameof(args));
            Guard.Against.Null(fileSystem, nameof(fileSystem));

            var requestedAssemblyName = new AssemblyName(args.Name);
            return LoadAssemblyFromDisk(requestedAssemblyName.Name!, fileSystem);
        }

        internal static Assembly LoadAssemblyFromDisk(string assemblyName, IFileSystem fileSystem)
        {
            Guard.Against.NullOrWhiteSpace(assemblyName, nameof(assemblyName));
            Guard.Against.Null(fileSystem, nameof(fileSystem));

            if (!fileSystem.Directory.Exists(SR.PlugInDirectoryPath))
            {
                throw new ConfigurationException($"Plug-in directory '{SR.PlugInDirectoryPath}' cannot be found.");
            }

            var assemblyFilePath = fileSystem.Path.Combine(SR.PlugInDirectoryPath, $"{assemblyName}.dll");
            if (!fileSystem.File.Exists(assemblyFilePath))
            {
                throw new ConfigurationException($"The configured plug-in '{assemblyFilePath}' cannot be found.");
            }

            var asesmblyeData = fileSystem.File.ReadAllBytes(assemblyFilePath);
            return Assembly.Load(asesmblyeData);
        }
    }
}
