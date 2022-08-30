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
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Moq;
using Xunit;

namespace Monai.Deploy.Messaging.RabbitMQ.Tests
{
#pragma warning disable CS8604 // Possible null reference argument.
    public class ValidationTest
    {

        [Fact(DisplayName = "Validates TaskUpdateEvent")]
        public void TaskUpdateEventTest()
        {
            //var json = "{\"workflow_instance_id\":\"6caf0cf6-75f8-4120-8117-8f5f0927eb5f\",\"task_id\":\"argo-task\",\"execution_id\":\"1f599e30-626a-4c7b-962a-fc221a574488\",\"correlation_id\":\"e4b06f00-5ce3-4477-86cb-4f3bf20680c2\",\"status\":\"Succeeded\",\"taskStats\":{\"workflowId\":\"6caf0cf6-75f8-4120-8117-8f5f0927eb5f\",\"duration\":-1,\"resourceDuration\":{\"cpu\":12,\"memory\":7},\"nodeInfo\":{\"md-wonderful-bear-f8hjz\":{\"children\":[\"md-wonderful-bear-f8hjz-190249463\"],\"displayName\":\"md-wonderful-bear-f8hjz\",\"finishedAt\":\"2022-07-18T17:13:21+01:00\",\"id\":\"md-wonderful-bear-f8hjz\",\"name\":\"md-wonderful-bear-f8hjz\",\"outboundNodes\":[\"md-wonderful-bear-f8hjz-2400127581\"],\"phase\":\"Succeeded\",\"progress\":\"1/1\",\"resourcesDuration\":{\"cpu\":3,\"memory\":2},\"startedAt\":\"2022-07-18T17:13:11+01:00\",\"templateName\":\"md-workflow-entrypoint\",\"templateScope\":\"local/md-wonderful-bear-f8hjz\",\"type\":\"Steps\"},\"md-wonderful-bear-f8hjz-190249463\":{\"boundaryID\":\"md-wonderful-bear-f8hjz\",\"children\":[\"md-wonderful-bear-f8hjz-2400127581\"],\"displayName\":\"[0]\",\"finishedAt\":\"2022-07-18T17:13:21+01:00\",\"id\":\"md-wonderful-bear-f8hjz-190249463\",\"name\":\"md-wonderful-bear-f8hjz[0]\",\"phase\":\"Succeeded\",\"progress\":\"1/1\",\"resourcesDuration\":{\"cpu\":3,\"memory\":2},\"startedAt\":\"2022-07-18T17:13:11+01:00\",\"templateScope\":\"local/md-wonderful-bear-f8hjz\",\"type\":\"StepGroup\"},\"md-wonderful-bear-f8hjz-2400127581\":{\"boundaryID\":\"md-wonderful-bear-f8hjz\",\"displayName\":\"md-workflow-entrypoint\",\"finishedAt\":\"2022-07-18T17:13:18+01:00\",\"hostNodeName\":\"docker-desktop\",\"id\":\"md-wonderful-bear-f8hjz-2400127581\",\"inputs\":{\"parameters\":[{\"name\":\"message\",\"value\":\"{{workflow.parameters.message}}\"}]},\"name\":\"md-wonderful-bear-f8hjz[0].md-workflow-entrypoint\",\"outputs\":{\"artifacts\":[{\"name\":\"main-logs\",\"s3\":{\"key\":\"md-wonderful-bear-f8hjz/md-wonderful-bear-f8hjz-argosay-2400127581/main.log\"}}],\"exitCode\":\"0\"},\"phase\":\"Succeeded\",\"progress\":\"1/1\",\"resourcesDuration\":{\"cpu\":3,\"memory\":2},\"startedAt\":\"2022-07-18T17:13:11+01:00\",\"templateName\":\"argosay\",\"templateScope\":\"local/md-wonderful-bear-f8hjz\",\"type\":\"Pod\"},\"md-wonderful-bear-f8hjz-2674993913\":{\"boundaryID\":\"md-wonderful-bear-f8hjz-2706448002\",\"children\":[\"md-wonderful-bear-f8hjz-3069151573\"],\"displayName\":\"[1]\",\"finishedAt\":\"2022-07-18T17:13:52+01:00\",\"id\":\"md-wonderful-bear-f8hjz-2674993913\",\"name\":\"md-wonderful-bear-f8hjz.onExit[1]\",\"phase\":\"Succeeded\",\"progress\":\"1/1\",\"resourcesDuration\":{\"cpu\":4,\"memory\":2},\"startedAt\":\"2022-07-18T17:13:42+01:00\",\"templateScope\":\"local/md-wonderful-bear-f8hjz\",\"type\":\"StepGroup\"},\"md-wonderful-bear-f8hjz-2706448002\":{\"children\":[\"md-wonderful-bear-f8hjz-3681798148\"],\"displayName\":\"md-wonderful-bear-f8hjz.onExit\",\"finishedAt\":\"2022-07-18T17:13:52+01:00\",\"id\":\"md-wonderful-bear-f8hjz-2706448002\",\"name\":\"md-wonderful-bear-f8hjz.onExit\",\"outboundNodes\":[\"md-wonderful-bear-f8hjz-3069151573\"],\"phase\":\"Succeeded\",\"progress\":\"2/2\",\"resourcesDuration\":{\"cpu\":9,\"memory\":5},\"startedAt\":\"2022-07-18T17:13:21+01:00\",\"templateName\":\"exit-message-template\",\"templateScope\":\"local/md-wonderful-bear-f8hjz\",\"type\":\"Steps\"},\"md-wonderful-bear-f8hjz-3069151573\":{\"boundaryID\":\"md-wonderful-bear-f8hjz-2706448002\",\"displayName\":\"send-message\",\"finishedAt\":\"2022-07-18T17:13:50+01:00\",\"hostNodeName\":\"docker-desktop\",\"id\":\"md-wonderful-bear-f8hjz-3069151573\",\"inputs\":{\"artifacts\":[{\"archive\":{\"none\":{}},\"name\":\"message\",\"path\":\"/tmp/b2d64917-32c5-4c1a-b299-3386dbc144eb.json\",\"s3\":{\"accessKeySecret\":{\"key\":\"accessKey\",\"name\":\"argo-task-637937575309296430\"},\"bucket\":\"test-bucket\",\"endpoint\":\"minio:9000\",\"insecure\":true,\"key\":\"00000000-1000-0000-0000-000000000000/workflows/6caf0cf6-75f8-4120-8117-8f5f0927eb5f/1f599e30-626a-4c7b-962a-fc221a574488/tmp/md-wonderful-bear-f8hjz/messaging/b2d64917-32c5-4c1a-b299-3386dbc144eb.json\",\"secretKeySecret\":{\"key\":\"secretKey\",\"name\":\"argo-task-637937575309296430\"}}}]},\"name\":\"md-wonderful-bear-f8hjz.onExit[1].send-message\",\"outputs\":{\"artifacts\":[{\"name\":\"main-logs\",\"s3\":{\"key\":\"md-wonderful-bear-f8hjz/md-wonderful-bear-f8hjz-send-message-3069151573/main.log\"}}],\"exitCode\":\"0\"},\"phase\":\"Succeeded\",\"progress\":\"1/1\",\"resourcesDuration\":{\"cpu\":4,\"memory\":2},\"startedAt\":\"2022-07-18T17:13:42+01:00\",\"templateName\":\"send-message\",\"templateScope\":\"local/md-wonderful-bear-f8hjz\",\"type\":\"Pod\"},\"md-wonderful-bear-f8hjz-3681798148\":{\"boundaryID\":\"md-wonderful-bear-f8hjz-2706448002\",\"children\":[\"md-wonderful-bear-f8hjz-743574115\"],\"displayName\":\"[0]\",\"finishedAt\":\"2022-07-18T17:13:42+01:00\",\"id\":\"md-wonderful-bear-f8hjz-3681798148\",\"name\":\"md-wonderful-bear-f8hjz.onExit[0]\",\"phase\":\"Succeeded\",\"progress\":\"2/2\",\"resourcesDuration\":{\"cpu\":9,\"memory\":5},\"startedAt\":\"2022-07-18T17:13:21+01:00\",\"templateScope\":\"local/md-wonderful-bear-f8hjz\",\"type\":\"StepGroup\"},\"md-wonderful-bear-f8hjz-743574115\":{\"boundaryID\":\"md-wonderful-bear-f8hjz-2706448002\",\"children\":[\"md-wonderful-bear-f8hjz-2674993913\"],\"displayName\":\"generate-message\",\"finishedAt\":\"2022-07-18T17:13:31+01:00\",\"hostNodeName\":\"docker-desktop\",\"id\":\"md-wonderful-bear-f8hjz-743574115\",\"inputs\":{\"parameters\":[{\"name\":\"event\",\"value\":\"{\\\"workflow_instance_id\\\":\\\"6caf0cf6-75f8-4120-8117-8f5f0927eb5f\\\",\\\"task_id\\\":\\\"argo-task\\\",\\\"execution_id\\\":\\\"1f599e30-626a-4c7b-962a-fc221a574488\\\",\\\"correlation_id\\\":\\\"e4b06f00-5ce3-4477-86cb-4f3bf20680c2\\\",\\\"identity\\\":\\\"md-wonderful-bear-f8hjz\\\",\\\"metadata\\\":{},\\\"outputs\\\":[]}\"},{\"name\":\"message\",\"value\":\"\\\"{\\\\\\\"ContentType\\\\\\\":\\\\\\\"application/json\\\\\\\",\\\\\\\"CorrelationID\\\\\\\":\\\\\\\"e4b06f00-5ce3-4477-86cb-4f3bf20680c2\\\\\\\",\\\\\\\"MessageID\\\\\\\":\\\\\\\"b2d64917-32c5-4c1a-b299-3386dbc144eb\\\\\\\",\\\\\\\"Type\\\\\\\":\\\\\\\"TaskCallbackEvent\\\\\\\",\\\\\\\"AppID\\\\\\\":\\\\\\\"Argo\\\\\\\",\\\\\\\"Exchange\\\\\\\":\\\\\\\"monaideploy\\\\\\\",\\\\\\\"RoutingKey\\\\\\\":\\\\\\\"md.tasks.callback\\\\\\\",\\\\\\\"DeliveryMode\\\\\\\":2,\\\\\\\"Body\\\\\\\":\\\\\\\"eyJ3b3JrZmxvd19pbnN0YW5jZV9pZCI6IjZjYWYwY2Y2LTc1ZjgtNDEyMC04MTE3LThmNWYwOTI3ZWI1ZiIsInRhc2tfaWQiOiJhcmdvLXRhc2siLCJleGVjdXRpb25faWQiOiIxZjU5OWUzMC02MjZhLTRjN2ItOTYyYS1mYzIyMWE1NzQ0ODgiLCJjb3JyZWxhdGlvbl9pZCI6ImU0YjA2ZjAwLTVjZTMtNDQ3Ny04NmNiLTRmM2JmMjA2ODBjMiIsImlkZW50aXR5IjoibWQtd29uZGVyZnVsLWJlYXItZjhoanoiLCJtZXRhZGF0YSI6e30sIm91dHB1dHMiOltdfQ==\\\\\\\"}\\\"\"}]},\"name\":\"md-wonderful-bear-f8hjz.onExit[0].generate-message\",\"outputs\":{\"artifacts\":[{\"archive\":{\"none\":{}},\"name\":\"output\",\"path\":\"/tmp\",\"s3\":{\"accessKeySecret\":{\"key\":\"accessKey\",\"name\":\"argo-task-637937575309296430\"},\"bucket\":\"test-bucket\",\"endpoint\":\"minio:9000\",\"insecure\":true,\"key\":\"00000000-1000-0000-0000-000000000000/workflows/6caf0cf6-75f8-4120-8117-8f5f0927eb5f/1f599e30-626a-4c7b-962a-fc221a574488/tmp/md-wonderful-bear-f8hjz/messaging\",\"secretKeySecret\":{\"key\":\"secretKey\",\"name\":\"argo-task-637937575309296430\"}}},{\"name\":\"main-logs\",\"s3\":{\"key\":\"md-wonderful-bear-f8hjz/md-wonderful-bear-f8hjz-generate-message-743574115/main.log\"}}],\"exitCode\":\"0\"},\"phase\":\"Succeeded\",\"progress\":\"1/1\",\"resourcesDuration\":{\"cpu\":5,\"memory\":3},\"startedAt\":\"2022-07-18T17:13:21+01:00\",\"templateName\":\"generate-message\",\"templateScope\":\"local/md-wonderful-bear-f8hjz\",\"type\":\"Pod\"}},\"startedAt\":\"2022-07-18T17:13:11+01:00\",\"finishedAt\":\"2022-07-18T17:13:52+01:00\"},\"reason\":\"None\",\"message\":\"\",\"outputs\":[],\"metadata\":{\"JobIdentity\":\"md-wonderful-bear-f8hjz\"}}";
            var json = """{"taskStats":{"workflowId":"6caf0cf6-75f8-4120-8117-8f5f0927eb5f","resourceDuration.cpu":12,"resourceDuration.memory":7}}""";
            var updateEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<TaskUpdateEvent>(json);
            var message = Assert.Throws<MessageValidationException>(() => updateEvent?.Validate());
            var expectedError = "Invalid message: The WorkflowInstanceId field is required. Path: WorkflowInstanceId.,The TaskId field is required. Path: TaskId.,The ExecutionId field is required. Path: ExecutionId.,The CorrelationId field is required. Path: CorrelationId.";
            Assert.Equal(expectedError, message.Message);
        }
    }

    public class PublisherServiceRegistrationTest : ServiceRegistrationTest<RabbitMQMessagePublisherService>
    {
        [Fact(DisplayName = "Shall be able to Add MinIO as default storage service")]
        public void ShallAddRabbitMQAsDefaultMessagingService()
        {
            var serviceCollection = new Mock<IServiceCollection>();
            serviceCollection.Setup(p => p.Add(It.IsAny<ServiceDescriptor>()));

            var returnedServiceCollection = serviceCollection.Object.AddMonaiDeployMessageBrokerPublisherService(ServiceType.AssemblyQualifiedName, FileSystem, false);

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

            var returnedServiceCollection = serviceCollection.Object.AddMonaiDeployMessageBrokerSubscriberService(ServiceType.AssemblyQualifiedName, FileSystem, false);

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
