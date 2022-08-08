/*
 * Copyright 2021-2022 MONAI Consortium
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
