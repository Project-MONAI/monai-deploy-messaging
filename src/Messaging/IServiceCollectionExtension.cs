// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.IO.Abstractions;
using System.Reflection;
using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Configuration;

namespace Monai.Deploy.Messaging
{
    public static class IServiceCollectionExtensions
    {
        private static IFileSystem? s_fileSystem;

        /// <summary>
        /// Configures all dependencies required for the MONAI Deploy Message Broker Subscriber Service.
        /// </summary>
        /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="fullyQualifiedTypeName">Fully qualified type name of the service to use.</param>
        /// <returns>Instance of <see cref="IServiceCollection"/>.</returns>
        /// <exception cref="ConfigurationException"></exception>
        public static IServiceCollection AddMonaiDeployMessageBrokerSubscriberService(this IServiceCollection services, string fullyQualifiedTypeName)
            => AddMonaiDeployMessageBrokerSubscriberService(services, fullyQualifiedTypeName, new FileSystem());

        /// <summary>
        /// Configures all dependencies required for the MONAI Deploy Message Broker Subscriber Service.
        /// </summary>
        /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="fullyQualifiedTypeName">Fully qualified type name of the service to use.</param>
        /// <param name="fileSystem">Instance of <see cref="IFileSystem"/>.</param>
        /// <returns>Instance of <see cref="IServiceCollection"/>.</returns>
        /// <exception cref="ConfigurationException"></exception>
        public static IServiceCollection AddMonaiDeployMessageBrokerSubscriberService(this IServiceCollection services, string fullyQualifiedTypeName, IFileSystem fileSystem)
            => Add<IMessageBrokerSubscriberService, SubscriberServiceRegistrationBase>(services, fullyQualifiedTypeName, fileSystem);

        /// <summary>
        /// Configures all dependencies required for the MONAI Deploy Message Broker Publisher Service.
        /// </summary>
        /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="fullyQualifiedTypeName">Fully qualified type name of the service to use.</param>
        /// <returns>Instance of <see cref="IServiceCollection"/>.</returns>
        /// <exception cref="ConfigurationException"></exception>
        public static IServiceCollection AddMonaiDeployMessageBrokerPublisherService(this IServiceCollection services, string fullyQualifiedTypeName)
            => AddMonaiDeployMessageBrokerPublisherService(services, fullyQualifiedTypeName, new FileSystem());

        /// <summary>
        /// Configures all dependencies required for the MONAI Deploy Message Broker Publisher Service.
        /// </summary>
        /// <param name="services">Instance of <see cref="IServiceCollection"/>.</param>
        /// <param name="fullyQualifiedTypeName">Fully qualified type name of the service to use.</param>
        /// <param name="fileSystem">Instance of <see cref="IFileSystem"/>.</param>
        /// <returns>Instance of <see cref="IServiceCollection"/>.</returns>
        /// <exception cref="ConfigurationException"></exception>
        public static IServiceCollection AddMonaiDeployMessageBrokerPublisherService(this IServiceCollection services, string fullyQualifiedTypeName, IFileSystem fileSystem)
            => Add<IMessageBrokerPublisherService, PublisherServiceRegistrationBase>(services, fullyQualifiedTypeName, fileSystem);

        private static IServiceCollection Add<T, U>(this IServiceCollection services, string fullyQualifiedTypeName, IFileSystem fileSystem) where U : ServiceRegistrationBase
        {
            Guard.Against.NullOrWhiteSpace(fullyQualifiedTypeName, nameof(fullyQualifiedTypeName));
            Guard.Against.Null(fileSystem, nameof(fileSystem));

            s_fileSystem = fileSystem;

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            try
            {
                var serviceAssembly = LoadAssemblyFromDisk(GetAssemblyName(fullyQualifiedTypeName));
                var serviceRegistrationType = serviceAssembly.GetTypes().FirstOrDefault(p => p.BaseType == typeof(U));

                if (serviceRegistrationType is null || Activator.CreateInstance(serviceRegistrationType, fullyQualifiedTypeName) is not U serviceRegistrar)
                {
                    throw new ConfigurationException($"Service registrar cannot be found for the configured plug-in '{fullyQualifiedTypeName}'.");
                }

                if (!IsSupportedType<T>(fullyQualifiedTypeName, serviceAssembly))
                {
                    throw new ConfigurationException($"The configured type '{fullyQualifiedTypeName}' does not implement the {typeof(T).Name} interface.");
                }

                return serviceRegistrar.Configure(services);
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            }
        }

        private static bool IsSupportedType<T>(string fullyQualifiedTypeName, Assembly storageServiceAssembly)
        {
            Guard.Against.NullOrWhiteSpace(fullyQualifiedTypeName, nameof(fullyQualifiedTypeName));
            Guard.Against.Null(storageServiceAssembly, nameof(storageServiceAssembly));

            var storageServiceType = Type.GetType(fullyQualifiedTypeName, assemblyeName => storageServiceAssembly, null, false);

            return storageServiceType is not null &&
                storageServiceType.GetInterfaces().Contains(typeof(T));
        }

        private static string GetAssemblyName(string fullyQualifiedTypeName)
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

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Guard.Against.Null(args, nameof(args));

            var requestedAssemblyName = new AssemblyName(args.Name);
            return LoadAssemblyFromDisk(requestedAssemblyName.Name);
        }

        private static Assembly LoadAssemblyFromDisk(string assemblyName)
        {
            Guard.Against.NullOrWhiteSpace(assemblyName, nameof(assemblyName));
            Guard.Against.Null(s_fileSystem, nameof(s_fileSystem));

            if (!s_fileSystem.Directory.Exists(SR.PlugInDirectoryPath))
            {
                throw new ConfigurationException($"Plug-in directory '{SR.PlugInDirectoryPath}' cannot be found.");
            }

            var assemblyFilePath = s_fileSystem.Path.Combine(SR.PlugInDirectoryPath, $"{assemblyName}.dll");
            if (!s_fileSystem.File.Exists(assemblyFilePath))
            {
                throw new ConfigurationException($"The configured plug-in '{assemblyFilePath}' cannot be found.");
            }

            var asesmblyeData = s_fileSystem.File.ReadAllBytes(assemblyFilePath);
            return Assembly.Load(asesmblyeData);
        }
    }
}
