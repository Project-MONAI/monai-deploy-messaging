using Microsoft.Extensions.Logging;

namespace Monai.Deploy.Messaging.SQS
{
    public static partial class Log
    {
        internal static readonly string LoggingScopeMessageApplication = "Message ID={0}. Application ID={1}.";


        [LoggerMessage(EventId = 10000, Level = LogLevel.Information, Message = "Publishing message {MessageId} to Queue={topic}.")]
        public static partial void PublishingToSQS(this ILogger logger, string topic, string MessageId);

        [LoggerMessage(EventId = 10001, Level = LogLevel.Information, Message = "{ServiceName} connecting to SQS.")]
        public static partial void ConnectingToSQS(this ILogger logger, string serviceName);

        [LoggerMessage(EventId = 10002, Level = LogLevel.Information, Message = "Message received from queue {queue}.")]
        public static partial void MessageReceivedFromQueue(this ILogger logger, string queue);

        [LoggerMessage(EventId = 10003, Level = LogLevel.Information, Message = "Listening for messages from {endpoint}. Queue={queue}.")]
        public static partial void SubscribeToSQSQueue(this ILogger logger, string endpoint, string queue);

        [LoggerMessage(EventId = 10004, Level = LogLevel.Information, Message = "Sending message acknowledgement for message {messageId}.")]
        public static partial void SendingAcknowledgement(this ILogger logger, string messageId);

        [LoggerMessage(EventId = 10005, Level = LogLevel.Information, Message = "Ackowledge sent for message {messageId}.")]
        public static partial void AcknowledgementSent(this ILogger logger, string messageId);

        [LoggerMessage(EventId = 10006, Level = LogLevel.Information, Message = "Sending nack message {messageId} and requeuing.")]
        public static partial void SendingNAcknowledgement(this ILogger logger, string messageId);

        [LoggerMessage(EventId = 10007, Level = LogLevel.Information, Message = "Nack message sent for message {messageId}, requeue={requeue}.")]
        public static partial void NAcknowledgementSent(this ILogger logger, string messageId, bool requeue);

        [LoggerMessage(EventId = 10008, Level = LogLevel.Information, Message = "Closing connections.")]
        public static partial void ClosingConnections(this ILogger logger);

        [LoggerMessage(EventId = 10009, Level = LogLevel.Error, Message = "Invalid or corrupted message received: Queue={queueName}, Message ID={messageId}.")]
        public static partial void InvalidMessage(this ILogger logger, string queueName, string messageId, Exception ex);

        [LoggerMessage(EventId = 10010, Level = LogLevel.Error, Message = "Exception not handled by the subscriber's callback function: Queue={queueName}, Message ID={messageId}.")]
        public static partial void ErrorNotHandledByCallback(this ILogger logger, string queueName, string messageId, Exception ex);

        [LoggerMessage(EventId = 10011, Level = LogLevel.Error, Message = "Creating SQS client.")]
        public static partial void CreateSQSClient(this ILogger logger);

        [LoggerMessage(EventId = 10012, Level = LogLevel.Error, Message = "{ServiceName}  failed to connect to SQS.")]
        public static partial void ConnectingToSQSError(this ILogger logger, string serviceName, Exception ex);
    }
}
