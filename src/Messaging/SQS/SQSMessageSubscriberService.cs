// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Text;
using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.ExtendedClient;
using Amazon.SQS.Model;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Configuration;
using Monai.Deploy.Messaging.Messages;
using Newtonsoft.Json.Linq;

namespace Monai.Deploy.Messaging.SQS
{
    public class SqsMessageSubscriberService : IMessageBrokerSubscriberService
    {
        private readonly ILogger<SqsMessageSubscriberService> _logger;
        private bool _disposedValue;
        private readonly string? _accessKey;
        private readonly string? _accessToken;
        private readonly string? _queueName;
        private readonly string _environmentId = string.Empty;

        private readonly AmazonSQSClient _sqsClient;
        private readonly AmazonS3Client _s3Client;
        private readonly AmazonSQSExtendedClient _sqSExtendedClient;

        public string Name => "AWS SQS Subscriber";

        public SqsMessageSubscriberService(IOptions<MessageBrokerServiceConfiguration> options,
                                                ILogger<SqsMessageSubscriberService> logger)
        {
            Guard.Against.Null(options, nameof(options));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var configuration = options.Value;
            ValidateConfiguration(configuration);
            _queueName = configuration.SubscriberSettings[SqsConfigurationKeys.ExportRequestQueue];
            string bucketName = configuration.SubscriberSettings[SqsConfigurationKeys.BucketName];


            if (configuration.SubscriberSettings.ContainsKey(SqsConfigurationKeys.AccessKey))
                _accessKey = configuration.SubscriberSettings[SqsConfigurationKeys.AccessKey];


            if (configuration.SubscriberSettings.ContainsKey(SqsConfigurationKeys.AccessToken))
                _accessToken = configuration.SubscriberSettings[SqsConfigurationKeys.AccessToken];


            if (configuration.SubscriberSettings.ContainsKey(SqsConfigurationKeys.Envid))
                _environmentId = configuration.SubscriberSettings[SqsConfigurationKeys.Envid];


            _logger.ConnectingToSQS(Name);

            if (!(_accessKey is null) && !(_accessToken is null))
            {
                _logger.LogInformation("Assuming IAM user as found in the configuration file.");
                _sqsClient = new AmazonSQSClient(_accessKey, _accessToken);
                _s3Client = new AmazonS3Client(_accessKey, _accessToken);
            }
            else
            {
                _logger.LogInformation("Attempting to assume local AWS credentials.");
                _sqsClient = new AmazonSQSClient();
                _s3Client = new AmazonS3Client();
            }

            _sqSExtendedClient = new AmazonSQSExtendedClient(_sqsClient,
            new ExtendedClientConfiguration().WithLargePayloadSupportEnabled(_s3Client, bucketName));


        }



        private void ValidateConfiguration(MessageBrokerServiceConfiguration configuration)
        {
            Guard.Against.Null(configuration, nameof(configuration));
            Guard.Against.Null(configuration.SubscriberSettings, nameof(configuration.SubscriberSettings));

            foreach (var key in SqsConfigurationKeys.SubscriberRequiredKeys)
            {
                if (!configuration.SubscriberSettings.ContainsKey(key))
                {
                    throw new ConfigurationException($"{Name} is missing configuration for {key}.");
                }
            }
        }

        public void Subscribe(string topic, string queue, Action<MessageReceivedEventArgs> messageReceivedCallback, ushort prefetchCount = 0)
            => Subscribe(new string[] { topic }, queue, messageReceivedCallback, prefetchCount);

        public void Subscribe(string[] topics, string queue, Action<MessageReceivedEventArgs> messageReceivedCallback, ushort prefetchCount = 0)
        {
            Guard.Against.Null(topics, nameof(topics));
            Guard.Against.Null(messageReceivedCallback, nameof(messageReceivedCallback));

            foreach (string topic in topics)
            {
                Task.Run(() =>
                {
                    QueueRunner(topic, messageReceivedCallback);
                });
            }

        }


        private void QueueRunner(string topic, Action<MessageReceivedEventArgs> messageReceivedCallback)
        {
            try
            {
                string queueName = QueueFormatter.FormatQueueName(_environmentId, _queueName, topic);
                _logger.LogDebug($"Attempting to create or subscribe to {queueName}");

                var queueAttributes = new Dictionary<string, string>();

                queueAttributes.Add("KmsMasterKeyId", "alias/aws/sqs");
                var request = new CreateQueueRequest
                {
                    Attributes = queueAttributes,
                    QueueName = queueName
                };

                CreateQueueResponse createQueueResponse = _sqSExtendedClient.CreateQueueAsync(request).Result;

                while (true)
                {
                    List<string> AttributesList = new List<string>();
                    AttributesList.Add("*");

                    var messageResponse = _sqSExtendedClient.ReceiveMessageAsync(new ReceiveMessageRequest
                    {
                        QueueUrl = createQueueResponse.QueueUrl,
                        WaitTimeSeconds = 2,
                        AttributeNames = new List<string> { "All" },
                        MessageAttributeNames = new List<string> { "All" }
                    }).Result;
                    var messages = messageResponse.Messages;

                    if (messages.Any())
                    {
                        foreach (var msg in messages)
                        {
                            _logger.Log(LogLevel.Debug, $"Message {msg.MessageId} received from SQS.");
                            MessageReceivedEventArgs messageReceivedEventArgs = CreateMessage(msg);
                            try
                            {
                                _logger.AcknowledgementSent(msg.MessageId);
                                _sqSExtendedClient.DeleteMessageAsync(new DeleteMessageRequest { QueueUrl = createQueueResponse.QueueUrl, ReceiptHandle = msg.ReceiptHandle }).Wait();
                                messageReceivedCallback(messageReceivedEventArgs);
                            }
                            catch (Exception ex)
                            {
                                _logger.Log(LogLevel.Error, ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.Message);
            }
        }

        public void SubscribeAsync(string topic, string queue, Func<MessageReceivedEventArgs, Task> messageReceivedCallback, ushort prefetchCount = 0)
            => SubscribeAsync(new string[] { topic }, queue, messageReceivedCallback, prefetchCount);

        public void SubscribeAsync(string[] topics, string queue, Func<MessageReceivedEventArgs, Task> messageReceivedCallback, ushort prefetchCount = 0)
        {
            throw new NotImplementedException();

        }

        public void Acknowledge(MessageBase message)
        {
            //No Acknowleddgement necessary with SQS. To delete the processed message is sufficient.
        }

        public void Reject(MessageBase message, bool requeue = true)
        {
            Guard.Against.Null(message, nameof(message));

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _sqSExtendedClient.Dispose();
                    _s3Client.Dispose();
                    _sqsClient.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        private static MessageReceivedEventArgs CreateMessage(Amazon.SQS.Model.Message msg)
        {

            Guard.Against.Null(msg, nameof(msg));
            Guard.Against.Null(msg.Body, nameof(msg.Body));
            Guard.Against.Null(msg.Attributes, nameof(msg.Attributes));
            Guard.Against.Null(msg.MessageId, nameof(msg.MessageId));

            JObject bodyobj = JObject.Parse(msg.Body);
            string messageid = bodyobj["MessageId"].ToString();
            string contentype = msg.MessageAttributes["ContentType"].ToString();
            string correlationId = bodyobj["correlation_id"].ToString();

            Guard.Against.Null(messageid, nameof(messageid));
            Guard.Against.Null(contentype, nameof(contentype));
            Guard.Against.Null(correlationId, nameof(correlationId));

            return new MessageReceivedEventArgs(
                new Monai.Deploy.Messaging.Messages.Message(
                body: Encoding.UTF8.GetBytes(msg.Body),
                messageDescription: msg.MessageAttributes["ContentType"].ToString(),
                messageId: messageid,
                applicationId: msg.Attributes["SenderId"],
                contentType: contentype,
                correlationId: correlationId,
                creationDateTime: DateTimeOffset.FromUnixTimeMilliseconds(Int64.Parse(msg.Attributes["SentTimestamp"])),
                deliveryTag: msg.ReceiptHandle)
                , CancellationToken.None);
        }
    }
}
