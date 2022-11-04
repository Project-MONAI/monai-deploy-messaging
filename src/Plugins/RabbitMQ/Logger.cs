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

using System;
using Microsoft.Extensions.Logging;

namespace Monai.Deploy.Messaging.RabbitMQ
{
    public static partial class Logger
    {
        internal static readonly string LoggingScopeMessageApplication = "Message ID={0}. Application ID={1}.";

        [LoggerMessage(EventId = 10000, Level = LogLevel.Information, Message = "Publishing message to {endpoint}/{virtualHost}. Exchange={exchange}, Routing Key={topic}.")]
        public static partial void PublshingRabbitMQ(this ILogger logger, string endpoint, string virtualHost, string exchange, string topic);

        [LoggerMessage(EventId = 10001, Level = LogLevel.Information, Message = "{ServiceName} connecting to {endpoint}/{virtualHost}.")]
        public static partial void ConnectingToRabbitMQ(this ILogger logger, string serviceNAme, string endpoint, string virtualHost);

        [LoggerMessage(EventId = 10002, Level = LogLevel.Information, Message = "Message received from queue {queue} for {topic}.")]
        public static partial void MessageReceivedFromQueue(this ILogger logger, string queue, string topic);

        [LoggerMessage(EventId = 10003, Level = LogLevel.Information, Message = "Listening for messages from {endpoint}/{virtualHost}. Exchange={exchange}, Queue={queue}, Routing Key={topic}.")]
        public static partial void SubscribeToRabbitMQQueue(this ILogger logger, string endpoint, string virtualHost, string exchange, string queue, string topic);

        [LoggerMessage(EventId = 10004, Level = LogLevel.Information, Message = "Sending message acknowledgement for message {messageId}.")]
        public static partial void SendingAcknowledgement(this ILogger logger, string messageId);

        [LoggerMessage(EventId = 10005, Level = LogLevel.Information, Message = "Ackowledge sent for message {messageId}. Event Duration {durationMilliseconds}")]
        public static partial void AcknowledgementSent(this ILogger logger, string messageId, double durationMilliseconds);

        [LoggerMessage(EventId = 10006, Level = LogLevel.Information, Message = "Sending nack message {messageId} and requeuing.")]
        public static partial void SendingNAcknowledgement(this ILogger logger, string messageId);

        [LoggerMessage(EventId = 10007, Level = LogLevel.Information, Message = "Nack message sent for message {messageId}, requeue={requeue}.")]
        public static partial void NAcknowledgementSent(this ILogger logger, string messageId, bool requeue);

        [LoggerMessage(EventId = 10008, Level = LogLevel.Information, Message = "Closing connections.")]
        public static partial void ClosingConnections(this ILogger logger);

        [LoggerMessage(EventId = 10009, Level = LogLevel.Error, Message = "Invalid or corrupted message received: Queue={queueName}, Topic={topic}, Message ID={messageId}.")]
        public static partial void InvalidMessage(this ILogger logger, string queueName, string topic, string messageId, Exception ex);

        [LoggerMessage(EventId = 10010, Level = LogLevel.Error, Message = "Exception not handled by the subscriber's callback function: Queue={queueName}, Topic={topic}, Message ID={messageId}.")]
        public static partial void ErrorNotHandledByCallback(this ILogger logger, string queueName, string topic, string messageId, Exception ex);

        [LoggerMessage(EventId = 10011, Level = LogLevel.Error, Message = "Exception thrown: Message ID={messageId}.")]
        public static partial void Exception(this ILogger logger, string messageId, Exception ex);

        [LoggerMessage(EventId = 10012, Level = LogLevel.Error, Message = "Health check failure.")]
        public static partial void HealthCheckError(this ILogger logger, Exception ex);
    }
}
