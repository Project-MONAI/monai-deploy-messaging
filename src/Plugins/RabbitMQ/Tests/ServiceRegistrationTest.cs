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

using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Monai.Deploy.Messaging.RabbitMQ.Tests
{
#pragma warning disable CS8604 // Possible null reference argument.

    public class PublisherServiceRegistrationTest : ServiceRegistrationTest<RabbitMQMessagePublisherService>
    {
        [Fact(DisplayName = "Shall be able to Add MinIO as default storage service")]
        public void ShallAddRabbitMQAsDefaultMessagingService()
        {
            var serviceCollection = new Mock<IServiceCollection>();
            serviceCollection.Setup(p => p.Add(It.IsAny<ServiceDescriptor>()));

            var returnedServiceCollection = serviceCollection.Object.AddMonaiDeployMessageBrokerPublisherService(ServiceType.AssemblyQualifiedName, FileSystem);

            Assert.Same(serviceCollection.Object, returnedServiceCollection);

            serviceCollection.Verify(p => p.Add(It.IsAny<ServiceDescriptor>()), Times.Exactly(2));
        }
    }

    public class SubscriberServiceRegistrationTest : ServiceRegistrationTest<RabbitMQMessageSubscriberService>
    {
        [Fact(DisplayName = "Shall be able to Add MinIO as default storage service")]
        public void ShallAddRabbitMQAsDefaultMessagingService()
        {
            var serviceCollection = new Mock<IServiceCollection>();
            serviceCollection.Setup(p => p.Add(It.IsAny<ServiceDescriptor>()));

            var returnedServiceCollection = serviceCollection.Object.AddMonaiDeployMessageBrokerSubscriberService(ServiceType.AssemblyQualifiedName, FileSystem);

            Assert.Same(serviceCollection.Object, returnedServiceCollection);

            serviceCollection.Verify(p => p.Add(It.IsAny<ServiceDescriptor>()), Times.Exactly(2));
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
