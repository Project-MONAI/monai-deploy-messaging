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

using Ardalis.GuardClauses;

namespace Monai.Deploy.Messaging.Messages
{
    public abstract class MessageBase
    {
        /// <summary>
        /// UUID for the message formatted with hyphens.
        /// xxxxxxxx-xxxx-Mxxx-Nxxx-xxxxxxxxxxxx
        /// </summary>
        public string MessageId { get; private set; }

        /// <summary>
        /// A short description of the type serialized in the message body.
        /// </summary>
        public string MessageDescription { get; private set; }

        /// <summary>
        /// Content or MIME type of the message body.
        /// </summary>
        public string ContentType { get; private set; }

        /// <summary>
        /// UUID of the application, in this case, the Informatics Gateway.
        /// The UUID of Informatics Gateway is <code>16988a78-87b5-4168-a5c3-2cfc2bab8e54</code>.
        /// </summary>
        public string ApplicationId { get; private set; }

        /// <summary>
        /// Correlation ID of the message.
        /// For DIMSE connections, the ID generated during association is used.
        /// For ACR inference requests, the Transaction ID provided in the request is used.
        /// </summary>
        public string CorrelationId { get; private set; }

        /// <summary>
        /// Datetime the message is created.
        /// </summary>
        public DateTimeOffset CreationDateTime { get; private set; }

        /// <summary>
        /// Gets or set the delivery tag/acknoweldge token for the message.
        /// </summary>
        public string DeliveryTag { get; protected set; }

        /// <summary>
        /// Gets or set the retry count of the message.
        /// </summary>
        public int RetryCount { get; set; }

        protected MessageBase(string messageId,
                              string messageDescription,
                              string contentType,
                              string applicationId,
                              string correlationId,
                              DateTimeOffset creationDateTime)
        {
            Guard.Against.NullOrWhiteSpace(messageId, nameof(messageId));
            Guard.Against.NullOrWhiteSpace(messageDescription, nameof(messageDescription));
            Guard.Against.NullOrWhiteSpace(contentType, nameof(contentType));
            Guard.Against.NullOrWhiteSpace(applicationId, nameof(applicationId));
            Guard.Against.NullOrWhiteSpace(correlationId, nameof(correlationId));

            MessageId = messageId;
            MessageDescription = messageDescription;
            ContentType = contentType;
            ApplicationId = applicationId;
            CorrelationId = correlationId;
            CreationDateTime = creationDateTime;
            DeliveryTag = string.Empty;
            RetryCount = 0;
        }
    }
}
