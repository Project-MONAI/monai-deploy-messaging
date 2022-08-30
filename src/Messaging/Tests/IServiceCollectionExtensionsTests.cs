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

using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Monai.Deploy.Messaging.API;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Messaging.Messages;
using Moq;
using Xunit;

namespace Monai.Deploy.Messaging.Tests
{
#pragma warning disable CS8604 // Possible null reference argument.

    public class IServiceCollectionExtensionsTests
    {
        [Theory(DisplayName = "AddMonaiDeployMessageBrokerServices throws when type name is invalid")]
        [InlineData("mytype")]
        [InlineData("mytype,, myversion")]
        [InlineData("mytype, myassembly, myversion")]
        public void AddMonaiDeployMessageBrokerServices_ThrowsOnInvalidTypeName(string typeName)
        {
            var serviceCollection = new Mock<IServiceCollection>();

            Assert.Throws<ConfigurationException>(() => serviceCollection.Object.AddMonaiDeployMessageBrokerPublisherService(typeName, new MockFileSystem()));
            Assert.Throws<ConfigurationException>(() => serviceCollection.Object.AddMonaiDeployMessageBrokerSubscriberService(typeName, new MockFileSystem()));
        }

        [Fact(DisplayName = "AddMonaiDeployMessageBrokerServices throws if the plug-ins directory is missing")]
        public void AddMonaiDeployMessageBrokerServices_ThrowsIfPlugInsDirectoryIsMissing()
        {
            var typeName = typeof(SomeClass).AssemblyQualifiedName;
            var serviceCollection = new Mock<IServiceCollection>();
            var exception = Assert.Throws<ConfigurationException>(() => serviceCollection.Object.AddMonaiDeployMessageBrokerPublisherService(typeName, new MockFileSystem()));
            Assert.NotNull(exception);
            Assert.Equal($"Plug-in directory '{SR.PlugInDirectoryPath}' cannot be found.", exception.Message);

            exception = Assert.Throws<ConfigurationException>(() => serviceCollection.Object.AddMonaiDeployMessageBrokerSubscriberService(typeName, new MockFileSystem()));
            Assert.NotNull(exception);
            Assert.Equal($"Plug-in directory '{SR.PlugInDirectoryPath}' cannot be found.", exception.Message);
        }

        [Fact(DisplayName = "AddMonaiDeployMessageBrokerServices throws if the plug-in dll is missing")]
        public void AddMonaiDeployMessageBrokerServices_ThrowsIfPlugInDllIsMissing()
        {
            var badType = typeof(SomeClass);
            var typeName = badType.AssemblyQualifiedName;
            var fileSystem = new MockFileSystem();
            fileSystem.Directory.CreateDirectory(SR.PlugInDirectoryPath);
            var serviceCollection = new Mock<IServiceCollection>();
            var exception = Assert.Throws<ConfigurationException>(() => serviceCollection.Object.AddMonaiDeployMessageBrokerPublisherService(typeName, fileSystem));
            Assert.NotNull(exception);
            Assert.Equal($"The configured plug-in '{SR.PlugInDirectoryPath}{Path.DirectorySeparatorChar}{badType.Assembly.ManifestModule.Name}' cannot be found.", exception.Message);

            exception = Assert.Throws<ConfigurationException>(() => serviceCollection.Object.AddMonaiDeployMessageBrokerSubscriberService(typeName, fileSystem));
            Assert.NotNull(exception);
            Assert.Equal($"The configured plug-in '{SR.PlugInDirectoryPath}{Path.DirectorySeparatorChar}{badType.Assembly.ManifestModule.Name}' cannot be found.", exception.Message);
        }

        [Fact(DisplayName = "AddMonaiDeployMessageBrokerServices throws if  service type is not supported")]
        public void AddMonaiDeployMessageBrokerServices_ThrowsIfServiceTypeIsNotSupported()
        {
            var badType = typeof(SomeClass);
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

            exception = Record.Exception(() => serviceCollection.Object.AddMonaiDeployMessageBrokerSubscriberService(typeName, fileSystem));
            Assert.NotNull(exception);
            Assert.Equal($"The configured type '{typeName}' does not implement the {typeof(IMessageBrokerSubscriberService).Name} interface.", exception.Message);
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
            var exception = Record.Exception(() => serviceCollection.Object.AddMonaiDeployMessageBrokerPublisherService(typeName, fileSystem, false));
            Assert.Null(exception);
            serviceCollection.Verify(p => p.Clear(), Times.Once());
        }

        [Fact(DisplayName = "AddMonaiDeployMessageBrokerSubscriberService configures all services as expected")]
        public void AddMonaiDeployMessageBrokerSubscriberService_ConfiuresServicesAsExpected()
        {
            var badType = typeof(GoodSubscriberService);
            var typeName = badType.AssemblyQualifiedName;
            var assemblyData = GetAssemblyeBytes(badType.Assembly);
            var assemblyFilePath = Path.Combine(SR.PlugInDirectoryPath, badType.Assembly.ManifestModule.Name);
            var fileSystem = new MockFileSystem();
            fileSystem.Directory.CreateDirectory(SR.PlugInDirectoryPath);
            fileSystem.File.WriteAllBytes(assemblyFilePath, assemblyData);
            var serviceCollection = new Mock<IServiceCollection>();
            serviceCollection.Setup(p => p.Clear());
            var exception = Record.Exception(() => serviceCollection.Object.AddMonaiDeployMessageBrokerSubscriberService(typeName, fileSystem, false));
            Assert.Null(exception);
            serviceCollection.Verify(p => p.Clear(), Times.Once());
        }

        private static byte[] GetAssemblyeBytes(Assembly assembly)
        {
            return File.ReadAllBytes(assembly.Location);
        }
    }

    internal class TestSubscriberServiceRegistrar : SubscriberServiceRegistrationBase
    {
        public TestSubscriberServiceRegistrar(string fullyQualifiedAssemblyName) : base(fullyQualifiedAssemblyName)
        {
        }

        public override IServiceCollection Configure(IServiceCollection services)
        {
            services.Clear();
            return services;
        }
    }

    internal class TestPublisherServiceRegistrar : PublisherServiceRegistrationBase
    {
        public TestPublisherServiceRegistrar(string fullyQualifiedAssemblyName) : base(fullyQualifiedAssemblyName)
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

    internal class GoodSubscriberService : IMessageBrokerSubscriberService
    {
        public string Name => throw new NotImplementedException();

        public void Acknowledge(MessageBase message) => throw new NotImplementedException();

        public void Dispose() => throw new NotImplementedException();

        public void Reject(MessageBase message, bool requeue = true) => throw new NotImplementedException();

        public Task RequeueWithDelay(MessageBase message) => throw new NotImplementedException();

        public void Subscribe(string topic, string queue, Action<MessageReceivedEventArgs> messageReceivedCallback, ushort prefetchCount = 0) => throw new NotImplementedException();

        public void Subscribe(string[] topics, string queue, Action<MessageReceivedEventArgs> messageReceivedCallback, ushort prefetchCount = 0) => throw new NotImplementedException();

        public void SubscribeAsync(string topic, string queue, Func<MessageReceivedEventArgs, Task> messageReceivedCallback, ushort prefetchCount = 0) => throw new NotImplementedException();

        public void SubscribeAsync(string[] topics, string queue, Func<MessageReceivedEventArgs, Task> messageReceivedCallback, ushort prefetchCount = 0) => throw new NotImplementedException();
    }

    internal class SomeClass
    {
    }

#pragma warning restore CS8604 // Possible null reference argument.
}
