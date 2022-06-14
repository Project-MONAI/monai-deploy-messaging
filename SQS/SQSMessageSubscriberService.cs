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
        private readonly string? _bucketName;
        private readonly string _environmentId = string.Empty;

        private readonly AmazonSQSClient? _sqsClient;
        private readonly AmazonS3Client? _s3Client;
        private readonly AmazonSQSExtendedClient _sqSExtendedClient;

        public string Name => "AWS SQS Subscriber";

        public SqsMessageSubscriberService(IOptions<MessageBrokerServiceConfiguration> options,
                                                ILogger<SqsMessageSubscriberService> logger)
        {
            Guard.Against.Null(options, nameof(options));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var configuration = options.Value;
            ValidateConfiguration(configuration);
            _queueName = configuration.SubscriberSettings[SQSConfigurationKeys.ExportRequestQueue];
            _bucketName = configuration.SubscriberSettings[SQSConfigurationKeys.BucketName];


            if (configuration.SubscriberSettings.ContainsKey(SQSConfigurationKeys.AccessKey))
                _accessKey = configuration.SubscriberSettings[SQSConfigurationKeys.AccessKey];


            if (configuration.SubscriberSettings.ContainsKey(SQSConfigurationKeys.AccessToken))
                _accessToken = configuration.SubscriberSettings[SQSConfigurationKeys.AccessToken];


            if (configuration.SubscriberSettings.ContainsKey(SQSConfigurationKeys.Envid))
                _environmentId = configuration.SubscriberSettings[SQSConfigurationKeys.Envid];

            try
            {
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
                new ExtendedClientConfiguration().WithLargePayloadSupportEnabled(_s3Client, _bucketName));

            }
            catch (Amazon.SQS.AmazonSQSException Ex)
            {
                _logger.ConnectingToSQSError(Name, Ex);
                Guard.Against.Null(_sqSExtendedClient, nameof(_sqSExtendedClient));
            }
        }



        private void ValidateConfiguration(MessageBrokerServiceConfiguration configuration)
        {
            Guard.Against.Null(configuration, nameof(configuration));
            Guard.Against.Null(configuration.SubscriberSettings, nameof(configuration.SubscriberSettings));

            foreach (var key in SQSConfigurationKeys.SubscriberRequiredKeys)
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


                    string queueName = QueueFormatter.FormatQueueName(_environmentId, _queueName, topic);
                    _logger.LogDebug($"Attempting to create or subscribe to {queueName}");

                    var queueAttributes = new Dictionary<string, string>();

                    queueAttributes.Add("KmsMasterKeyId", "alias/aws/sqs");
                    var request = new CreateQueueRequest
                    {
                        Attributes = queueAttributes,
                        QueueName = queueName
                    };

                    CreateQueueResponse createQueueResponse = new CreateQueueResponse();
                    try
                    {
                        createQueueResponse = _sqSExtendedClient.CreateQueueAsync(request).Result;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug($"The queue could not be created or subscribed to: {ex.Message}");
                    }


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
                });
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
            //We will use this to delelete the message from the SQS qeue.
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

            return new MessageReceivedEventArgs(
                new Monai.Deploy.Messaging.Messages.Message(
                body: Encoding.UTF8.GetBytes(msg.Body),
                messageDescription: "desc1",
                messageId: bodyobj["MessageId"].ToString(),
                applicationId: msg.Attributes["SenderId"],
                contentType: msg.MessageAttributes["ContentType"].ToString(),
                correlationId: bodyobj["correlation_id"].ToString(),
                creationDateTime: DateTimeOffset.FromUnixTimeMilliseconds(Int64.Parse(msg.Attributes["SentTimestamp"])),
                deliveryTag: msg.ReceiptHandle)
                , CancellationToken.None);
        }
    }
}
