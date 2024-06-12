/*
 * Copyright 2021-2024 MONAI Consortium
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

using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Messages;

namespace Monai.Deploy.Messaging.API
{
    public interface IMessageBrokerSubscriberService : IDisposable
    {
        /// <summary>
        /// Gets or sets the event delegate for client to handle connection errors.
        /// </summary>
        event ConnectionErrorHandler? OnConnectionError;

        /// <summary>
        /// Gets or sets the name of the storage service.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Subscribe to a message topic and queue and executes <c>messageReceivedCallback</c> asynchronously for every message that is received.
        /// Either provide a topic, a queue or both.
        /// A queue is generated if the name of the queue is not provided.
        /// </summary>
        /// <param name="topic">Topic/routing key to bind to</param>
        /// <param name="queue">Name of the queue to consume</param>
        /// <param name="messageReceivedCallback"><see cref="Func{MessageReceivedEventArgs, Task}"/> to be performed when message is received</param>
        /// <param name="prefetchCount">Number of unacknowledged messages to receive at once. Defaults to 0.</param>
        void SubscribeAsync(string topic, string queue, Func<MessageReceivedEventArgs, Task> messageReceivedCallback, ushort prefetchCount = 0);

        /// <summary>
        /// Subscribe to a message topic and queue and executes <c>messageReceivedCallback</c> asynchronously for every message that is received.
        /// Either provide a topic, a queue or both.
        /// A queue is generated if the name of the queue is not provided.
        /// </summary>
        /// <param name="topics">Topics/routing keys to bind to</param>
        /// <param name="queue">Name of the queue to consume</param>
        /// <param name="messageReceivedCallback"><see cref="Func{MessageReceivedEventArgs, Task}"/> to be performed when message is received</param>
        /// <param name="prefetchCount">Number of unacknowledged messages to receive at once. Defaults to 0.</param>
        void SubscribeAsync(string[] topics, string queue, Func<MessageReceivedEventArgs, Task> messageReceivedCallback, ushort prefetchCount = 0);

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

        /// <summary>
        /// Rejects a message and requeues with a configured delay.
        /// </summary>
        /// <param name="message">Message to be rejected.</param>
        Task RequeueWithDelay(MessageBase message);
    }
}
