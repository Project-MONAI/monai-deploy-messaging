// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

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
                       string deliveryTag = "")
            : base(messageId, messageDescription, contentType, applicationId, correlationId, creationDateTime)
        {
            Body = body;
            DeliveryTag = deliveryTag;
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
                return new JsonMessage<T>(body, MessageDescription, MessageId, ApplicationId, CorrelationId, CreationDateTime, DeliveryTag);
            }
            catch (Exception ex)
            {
                throw new MessageConversionException($"Error converting message to type {typeof(T)}", ex);
            }
        }
    }
}
