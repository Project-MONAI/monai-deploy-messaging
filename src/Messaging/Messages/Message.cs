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

using System.Text;
using Monai.Deploy.Messaging.Common;
using Newtonsoft.Json;

namespace Monai.Deploy.Messaging.Messages
{
    public sealed class Message : MessageBase
    {
        /// <summary>
        /// Body of the message.
        /// </summary>
        public byte[] Body { get; private set; }

        public Message(byte[] body,
                       string messageDescription,
                       string messageId,
                       string applicationId,
                       string contentType,
                       string correlationId,
                       DateTimeOffset creationDateTime,
                       string deliveryTag = "",
                       int retryCount = 0)
            : base(messageId, messageDescription, contentType, applicationId, correlationId, creationDateTime)
        {
            Body = body;
            DeliveryTag = deliveryTag;
            RetryCount = retryCount;
        }

        /// <summary>
        /// Converts <c>Body</c> from binary[] to JSON string and then the specified <c>T</c> type.
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <returns>Instance of <c>T</c> or <c>null</c> if data cannot be deserialized.</returns>
        public T ConvertTo<T>()
        {
            try
            {
                var json = Encoding.UTF8.GetString(Body);
                return JsonConvert.DeserializeObject<T>(json)!;
            }
            catch (Exception ex)
            {
                throw new MessageConversionException($"Error converting message to type {typeof(T)}", ex);
            }
        }

        /// <summary>
        /// Converts the <c>Message</c> into a <c>JsonMessage<typeparamref name="T"/>T</c>.
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <returns>Instance of <c>JsonMessage<typeparamref name="T"/>T</c> or <c>null</c> if data cannot be deserialized.</returns>
        public JsonMessage<T> ConvertToJsonMessage<T>()
        {
            try
            {
                var body = ConvertTo<T>();
                return new JsonMessage<T>(body, MessageDescription, MessageId, ApplicationId, CorrelationId, CreationDateTime, DeliveryTag, RetryCount);
            }
            catch (Exception ex)
            {
                throw new MessageConversionException($"Error converting message to type {typeof(T)}", ex);
            }
        }
    }
}
