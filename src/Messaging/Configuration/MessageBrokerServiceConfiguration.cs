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

using Microsoft.Extensions.Configuration;

namespace Monai.Deploy.Messaging.Configuration
{
    public class MessageBrokerServiceConfiguration
    {
        public const string DefaultPublisherAssemblyName = "Monai.Deploy.Messaging.RabbitMQ.RabbitMQMessagePublisherService, Monai.Deploy.Messaging.RabbitMQ";
        public const string DefaultSubscriberAssemblyName = "Monai.Deploy.Messaging.RabbitMQ.RabbitMQMessageSubscriberService, Monai.Deploy.Messaging.RabbitMQ";

        /// <summary>
        /// Gets or sets the a fully qualified type name of the message publisher service.
        /// The spcified type must implement <typeparam name="Monai.Deploy.InformaticsGateway.Api.MessageBroker.IMessageBrokerPublisherService">IMessageBrokerPublisherService</typeparam> interface.
        /// The default message publisher service configured is RabbitMQ.
        /// </summary>
        [ConfigurationKeyName("publisherServiceAssemblyName")]
        public string PublisherServiceAssemblyName { get; set; } = DefaultPublisherAssemblyName;

        /// <summary>
        /// Gets or sets the a fully qualified type name of the message subscriber service.
        /// The spcified type must implement <typeparam name="Monai.Deploy.InformaticsGateway.Api.MessageBroker.IMessageBrokerSubscriberService">IMessageBrokerSubscriberService</typeparam> interface.
        /// The default message subscriber service configured is RabbitMQ.
        /// </summary>
        [ConfigurationKeyName("subscriberServiceAssemblyName")]
        public string SubscriberServiceAssemblyName { get; set; } = DefaultSubscriberAssemblyName;

        /// <summary>
        /// Gets or sets the message publisher specific settings.
        /// Service implementer shall validate settings in the constructor and specify all settings in a single level JSON object as in the example below.
        /// </summary>
        /// <example>
        /// <code>
        /// {
        ///     ...
        ///     "publisherSettings": {
        ///         "endpoint": "1.2.3.4",
        ///         "username": "monaideploy",
        ///         "password": "mysecret",
        ///         "setting-a": "value-a",
        ///         "setting-b": "value-b"
        ///     }
        /// }
        /// </code>
        /// </example>
        [ConfigurationKeyName("publisherSettings")]
        public Dictionary<string, string> PublisherSettings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the message subscriber specific settings.
        /// Service implementer shall validate settings in the constructor and specify all settings in a single level JSON object as in the example below.
        /// </summary>
        /// <example>
        /// <code>
        /// {
        ///     ...
        ///     "subscriberSettings": {
        ///         "endpoint": "1.2.3.4",
        ///         "username": "monaideploy",
        ///         "password": "myothersecret",
        ///         "setting-a": "value-a",
        ///         "setting-b": "value-b"
        ///     }
        /// }
        /// </code>
        /// </example>
        [ConfigurationKeyName("subscriberSettings")]
        public Dictionary<string, string> SubscriberSettings { get; set; } = new Dictionary<string, string>();
    }
}
