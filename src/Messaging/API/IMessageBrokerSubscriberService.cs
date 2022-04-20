// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Messages;

namespace Monai.Deploy.Messaging
{
    public interface IMessageBrokerSubscriberService : IDisposable
    {
        /// <summary>
        /// Gets or sets the name of the storage service.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Subscribe to a message topic & queue and executes <c>messageReceivedCallback</c> for every message that is received.
        /// Either provide a topic, a queue or both.
        /// A queue is generated if the name of the queue is not provided.
        /// </summary>
        /// <param name="topic">Name of the topic to subscribe to</param>
        /// <param name="queue">Name of the queue to consume</param>
        /// <param name="messageReceivedCallback">Action to be performed when message is received</param>
        /// <param name="prefetchCount">Number of unacknowledged messages to receive at once. Defaults to 0.</param>
        void Subscribe(string topic, string queue, Action<MessageReceivedEventArgs> messageReceivedCallback, ushort prefetchCount = 0);

        /// <summary>
        /// Subscribe to a message topic & queue and executes <c>messageReceivedCallback</c> asynchronously for every message that is received.
        /// Either provide a topic, a queue or both.
        /// A queue is generated if the name of the queue is not provided.
        /// </summary>
        /// <param name="topic">Name of the topic to subscribe to</param>
        /// <param name="queue">Name of the queue to consume</param>
        /// <param name="messageReceivedCallback"><see cref="Func{MessageReceivedEventArgs, Task}"/> to be performed when message is received</param>
        /// <param name="prefetchCount">Number of unacknowledged messages to receive at once. Defaults to 0.</param>
        void SubscribeAsync(string topic, string queue, Func<MessageReceivedEventArgs, Task> messageReceivedCallback, ushort prefetchCount = 0);

        /// <summary>
        /// Acknowledge receiving of a message with the given token.
        /// </summary>
        /// <param name="message">Message to be acknowledged.</param>
        void Acknowledge(MessageBase message);

        /// <summary>
        /// Rejects a message.
        /// </summary>
        /// <param name="message">Message to be rejected.</param>
        /// <param name="requeue">Determines if the message should be requeued.</param>
        void Reject(MessageBase message, bool requeue = true);
    }
}
