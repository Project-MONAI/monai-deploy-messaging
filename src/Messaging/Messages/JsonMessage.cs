// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Net.Mime;
using System.Text;
using Ardalis.GuardClauses;
using Newtonsoft.Json;

namespace Monai.Deploy.Messaging.Messages
{
    public sealed class JsonMessage<T> : MessageBase
    {
        /// <summary>
        /// Body of the message.
        /// </summary>
        public T Body { get; private set; }

        public JsonMessage(T body,
                       string applicationId,
                       string correlationId,
                       string deliveryTag = "")
            : this(body,
                   body?.GetType().Name ?? default!,
                   Guid.NewGuid().ToString(),
                   applicationId,
                   correlationId,
                   DateTimeOffset.UtcNow,
                   deliveryTag)
        {
        }

        public JsonMessage(T body,
                       string messageDescription,
                       string messageId,
                       string applicationId,
                       string correlationId,
                       DateTimeOffset creationDateTime,
                       string deliveryTag,
                       int retryCount = 0)
            : base(messageId, messageDescription, MediaTypeNames.Application.Json, applicationId, correlationId, creationDateTime)
        {
            Guard.Against.Null(body, nameof(body));

            Body = body;
            DeliveryTag = deliveryTag;
            RetryCount = retryCount;
        }

        /// <summary>
        /// Converts <c>Body</c> to JSON and then binary[].
        /// </summary>
        /// <returns></returns>
        public Message ToMessage()
        {
            var json = JsonConvert.SerializeObject(Body);

            return new Message(
                Encoding.UTF8.GetBytes(json),
                Body?.GetType().Name ?? default!,
                MessageId,
                ApplicationId,
                ContentType,
                CorrelationId,
                CreationDateTime,
                DeliveryTag,
                RetryCount);
        }
    }
}
