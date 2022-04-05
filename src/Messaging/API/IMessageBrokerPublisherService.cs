// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Messages;

namespace Monai.Deploy.Messaging
{
    public interface IMessageBrokerPublisherService
    {
        /// <summary>
        /// Gets or sets the name of the storage service.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Publishes a message to the service.
        /// </summary>
        /// <param name="topic">Topic where the message is published to</param>
        /// <param name="message">Message to be published</param>
        /// <returns></returns>
        Task Publish(string topic, Message message);
    }
}
