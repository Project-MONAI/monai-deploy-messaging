// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Messaging.Messages;
using Moq;
using Xunit;

namespace Monai.Deploy.Messaging.Tests
{
#pragma warning disable CS8604 // Possible null reference argument.

    public class IServiceCollectionExtensionsTests
    {
        [Theory(DisplayName = "AddMonaiDeployMessageBrokerPublisherService throws when type name is invalid")]
        [InlineData("mytype")]
        [InlineData("mytype,, myversion")]
        [InlineData("mytype, myassembly, myversion")]
        public void AddMonaiDeployMessageBrokerPublisherService_ThrowsOnInvalidTypeName(string typeName)
        {
            var serviceCollection = new Mock<IServiceCollection>();

            Assert.Throws<ConfigurationException>(() => serviceCollection.Object.AddMonaiDeployMessageBrokerPublisherService(typeName, new MockFileSystem()));
        }

        [Fact(DisplayName = "AddMonaiDeployMessageBrokerPublisherService throws if the plug-ins directory is missing")]
        public void AddMonaiDeployMessageBrokerPublisherService_ThrowsIfPlugInsDirectoryIsMissing()
        {
            var typeName = typeof(TheBadTestPublisherService).AssemblyQualifiedName;
            var serviceCollection = new Mock<IServiceCollection>();
            var exception = Assert.Throws<ConfigurationException>(() => serviceCollection.Object.AddMonaiDeployMessageBrokerPublisherService(typeName, new MockFileSystem()));

            Assert.NotNull(exception);
            Assert.Equal($"Plug-in directory '{SR.PlugInDirectoryPath}' cannot be found.", exception.Message);
        }

        [Fact(DisplayName = "AddMonaiDeployMessageBrokerPublisherService throws if the plug-in dll is missing")]
        public void AddMonaiDeployMessageBrokerPublisherService_ThrowsIfPlugInDllIsMissing()
        {
            var badType = typeof(TheBadTestPublisherService);
            var typeName = badType.AssemblyQualifiedName;
            var fileSystem = new MockFileSystem();
            fileSystem.Directory.CreateDirectory(SR.PlugInDirectoryPath);
            var serviceCollection = new Mock<IServiceCollection>();
            var exception = Assert.Throws<ConfigurationException>(() => serviceCollection.Object.AddMonaiDeployMessageBrokerPublisherService(typeName, fileSystem));

            Assert.NotNull(exception);
            Assert.Equal($"The configured storage plug-in '{SR.PlugInDirectoryPath}{Path.DirectorySeparatorChar}{badType.Assembly.ManifestModule.Name}' cannot be found.", exception.Message);
        }

        [Fact(DisplayName = "AddMonaiDeployMessageBrokerPublisherService throws if service registrar cannot be found in the assembly")]
        public void AddMonaiDeployMessageBrokerPublisherService_ThrowsIfServiceRegistrarCannotBeFoundInTheAssembly()
        {
            var badType = typeof(Assert);
            var typeName = badType.AssemblyQualifiedName;
            var assemblyData = GetAssemblyeBytes(badType.Assembly);
            var assemblyFilePath = Path.Combine(SR.PlugInDirectoryPath, badType.Assembly.ManifestModule.Name);
            var fileSystem = new MockFileSystem();
            fileSystem.Directory.CreateDirectory(SR.PlugInDirectoryPath);
            fileSystem.File.WriteAllBytes(assemblyFilePath, assemblyData);
            var serviceCollection = new Mock<IServiceCollection>();
            var exception = Assert.Throws<ConfigurationException>(() => serviceCollection.Object.AddMonaiDeployMessageBrokerPublisherService(typeName, fileSystem));

            Assert.NotNull(exception);
            Assert.Equal($"Service registrar cannot be found for the configured plug-in '{typeName}'.", exception.Message);
        }

        [Fact(DisplayName = "AddMonaiDeployMessageBrokerPublisherService throws if storage service type is not supported")]
        public void AddMonaiDeployMessageBrokerPublisherService_ThrowsIfStorageServiceTypeIsNotSupported()
        {
            var badType = typeof(TheBadTestPublisherService);
            var typeName = badType.AssemblyQualifiedName;
            var assemblyData = GetAssemblyeBytes(badType.Assembly);
            var assemblyFilePath = Path.Combine(SR.PlugInDirectoryPath, badType.Assembly.ManifestModule.Name);
            var fileSystem = new MockFileSystem();
            fileSystem.Directory.CreateDirectory(SR.PlugInDirectoryPath);
            fileSystem.File.WriteAllBytes(assemblyFilePath, assemblyData);
            var serviceCollection = new Mock<IServiceCollection>();
            serviceCollection.Setup(p => p.Clear());
            var exception = Record.Exception(() => serviceCollection.Object.AddMonaiDeployMessageBrokerPublisherService(typeName, fileSystem));

            Assert.NotNull(exception);
            Assert.Equal($"The configured type '{typeName}' does not implement the {typeof(IMessageBrokerPublisherService).Name} interface.", exception.Message);
        }

        [Fact(DisplayName = "AddMonaiDeployMessageBrokerPublisherService configures all services as expected")]
        public void AddMonaiDeployMessageBrokerPublisherService_ConfiuresServicesAsExpected()
        {
            var badType = typeof(GoodPublisherService);
            var typeName = badType.AssemblyQualifiedName;
            var assemblyData = GetAssemblyeBytes(badType.Assembly);
            var assemblyFilePath = Path.Combine(SR.PlugInDirectoryPath, badType.Assembly.ManifestModule.Name);
            var fileSystem = new MockFileSystem();
            fileSystem.Directory.CreateDirectory(SR.PlugInDirectoryPath);
            fileSystem.File.WriteAllBytes(assemblyFilePath, assemblyData);
            var serviceCollection = new Mock<IServiceCollection>();
            serviceCollection.Setup(p => p.Clear());
            var exception = Record.Exception(() => serviceCollection.Object.AddMonaiDeployMessageBrokerPublisherService(typeName, fileSystem));

            Assert.Null(exception);

            serviceCollection.Verify(p => p.Clear(), Times.Once());
        }

        private static byte[] GetAssemblyeBytes(Assembly assembly)
        {
            return File.ReadAllBytes(assembly.Location);
        }
    }

    internal class TestServiceRegistrar : PublisherServiceRegistrationBase
    {
        public TestServiceRegistrar(string fullyQualifiedAssemblyName) : base(fullyQualifiedAssemblyName)
        {
        }

        public override IServiceCollection Configure(IServiceCollection services)
        {
            services.Clear();
            return services;
        }
    }

    internal class GoodPublisherService : IMessageBrokerPublisherService
    {
        public string Name => throw new NotImplementedException();

        public void Dispose() => throw new NotImplementedException();

        public Task Publish(string topic, Message message) => throw new NotImplementedException();
    }

    internal class TheBadTestPublisherService
    {
    }

#pragma warning restore CS8604 // Possible null reference argument.
}
