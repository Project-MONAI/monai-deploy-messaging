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

using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Moq;
using Xunit;

namespace Monai.Deploy.Messaging.RabbitMQ.Tests.Unit
{
#pragma warning disable CS8604 // Possible null reference argument.

    public class ValidationTest
    {
        [Fact(DisplayName = "Validates TaskUpdateEvent")]
        public void TaskUpdateEventTest()
        {
            var json = "{'taskStats':{'workflowId':'6caf0cf6-75f8-4120-8117-8f5f0927eb5f','resourceDuration.cpu':12,'resourceDuration.memory':7}}";
            var updateEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<TaskUpdateEvent>(json);
            var message = Assert.Throws<MessageValidationException>(() => updateEvent?.Validate());
            var expectedError = "Invalid message: The WorkflowInstanceId field is required. Path: WorkflowInstanceId.,The TaskId field is required. Path: TaskId.,The ExecutionId field is required. Path: ExecutionId.,The CorrelationId field is required. Path: CorrelationId.";
            Assert.Equal(expectedError, message.Message);
        }
    }

    public class PublisherServiceRegistrationTest : ServiceRegistrationTest<RabbitMQMessagePublisherService>
    {
        [Fact]
        public void ShallAddRabbitMQAsDefaultMessagingService()
        {
            var serviceCollection = new Mock<IServiceCollection>();
            serviceCollection.Setup(p => p.Add(It.IsAny<ServiceDescriptor>()));

            var returnedServiceCollection = serviceCollection.Object.AddMonaiDeployMessageBrokerPublisherService(ServiceType.AssemblyQualifiedName, FileSystem, false);

            Assert.Same(serviceCollection.Object, returnedServiceCollection);

            serviceCollection.Verify(p => p.Add(It.IsAny<ServiceDescriptor>()), Times.Exactly(2));
        }

        [Fact]
        public void ShallAddRabbitMQAsDefaultMessagingServicAndStorageHealthCheckse()
        {
            var serviceCollection = new Mock<IServiceCollection>();
            serviceCollection.Setup(p => p.Add(It.IsAny<ServiceDescriptor>()));

            var returnedServiceCollection = serviceCollection.Object.AddMonaiDeployMessageBrokerPublisherService(ServiceType.AssemblyQualifiedName, FileSystem, true);

            Assert.Same(serviceCollection.Object, returnedServiceCollection);

            serviceCollection.Verify(p => p.Add(It.IsAny<ServiceDescriptor>()), Times.AtLeast(3));
            serviceCollection.Verify(p => p.Add(It.Is<ServiceDescriptor>(p => p.ServiceType == typeof(HealthCheckService))), Times.Once());
        }
    }

    public class SubscriberServiceRegistrationTest : ServiceRegistrationTest<RabbitMQMessageSubscriberService>
    {
        [Fact]
        public void AddMonaiDeployMessageBrokerSubscriberService_WhenCalled_RegisterServicesOnly()
        {
            var serviceCollection = new Mock<IServiceCollection>();
            serviceCollection.Setup(p => p.Add(It.IsAny<ServiceDescriptor>()));

            var returnedServiceCollection = serviceCollection.Object.AddMonaiDeployMessageBrokerSubscriberService(ServiceType.AssemblyQualifiedName, FileSystem, false);

            Assert.Same(serviceCollection.Object, returnedServiceCollection);

            serviceCollection.Verify(p => p.Add(It.IsAny<ServiceDescriptor>()), Times.Exactly(2));
        }

        [Fact]
        public void AddMonaiDeployMessageBrokerSubscriberService_WhenCalled_RegisterServicesAndHealthChecks()
        {
            var serviceCollection = new Mock<IServiceCollection>();
            serviceCollection.Setup(p => p.Add(It.IsAny<ServiceDescriptor>()));

            var returnedServiceCollection = serviceCollection.Object.AddMonaiDeployMessageBrokerSubscriberService(ServiceType.AssemblyQualifiedName, FileSystem, true);

            Assert.Same(serviceCollection.Object, returnedServiceCollection);

            serviceCollection.Verify(p => p.Add(It.IsAny<ServiceDescriptor>()), Times.AtLeast(3));
            serviceCollection.Verify(p => p.Add(It.Is<ServiceDescriptor>(p => p.ServiceType == typeof(HealthCheckService))), Times.Once());
        }
    }

    public abstract class ServiceRegistrationTest<T>
    {
        protected Type ServiceType { get; }

        protected MockFileSystem FileSystem { get; }

        protected ServiceRegistrationTest()
        {
            ServiceType = typeof(T);
            FileSystem = new MockFileSystem();
            var assemblyFilePath = Path.Combine(SR.PlugInDirectoryPath, ServiceType.Assembly.ManifestModule.Name);
            var assemblyData = GetAssemblyeBytes(ServiceType.Assembly);
            FileSystem.Directory.CreateDirectory(SR.PlugInDirectoryPath);
            FileSystem.File.WriteAllBytes(assemblyFilePath, assemblyData);
        }

        private static byte[] GetAssemblyeBytes(Assembly assembly)
        {
            return File.ReadAllBytes(assembly.Location);
        }

        protected void AddOptions(Dictionary<string, string> settings, string[] requiredKeys)
        {
            foreach (var key in requiredKeys)
            {
                if (settings.ContainsKey(key)) continue;

                settings.Add(key, Guid.NewGuid().ToString());
            }
        }
    }

#pragma warning restore CS8604 // Possible null reference argument.
}
