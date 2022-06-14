// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Globalization;
using System.Text;
using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.ExtendedClient;
using Amazon.SQS.Model;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monai.Deploy.Messaging.Configuration;

namespace Monai.Deploy.Messaging.SQS
{
    public class SqsMessagePublisherService : IMessageBrokerPublisherService
    {

        private readonly ILogger<SqsMessagePublisherService> _logger;
        private readonly string? _accessKey;
        private readonly string? _accessToken;
        private readonly string _environmentId = string.Empty;
        private bool _disposedValue;


        public string Name => "AWS SQS Publisher";
        private readonly string _queueName;
        private readonly AmazonSQSClient _sqsClient;
        private readonly AmazonS3Client _s3Client;
        private readonly AmazonSQSExtendedClient _sqSExtendedClient;

        public SqsMessagePublisherService(IOptions<MessageBrokerServiceConfiguration> options,
                                               ILogger<SqsMessagePublisherService> logger)
        {
            Guard.Against.Null(options, nameof(options));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var configuration = options.Value;
            ValidateConfiguration(configuration);

            _queueName = configuration.PublisherSettings[SqsConfigurationKeys.WorkflowRequestQueue];
            string bucketName = configuration.PublisherSettings[SqsConfigurationKeys.BucketName];


            if (configuration.PublisherSettings.ContainsKey(SqsConfigurationKeys.AccessKey))
            {
                _logger.LogInformation("accessKey found in configuration.");
                _accessKey = configuration.PublisherSettings[SqsConfigurationKeys.AccessKey];
            }


            if (configuration.PublisherSettings.ContainsKey(SqsConfigurationKeys.AccessToken))
            {
                _logger.LogInformation("accessToken found in configuration.");
                _accessToken = configuration.PublisherSettings[SqsConfigurationKeys.AccessToken];
            }

            if (configuration.PublisherSettings.ContainsKey(SqsConfigurationKeys.Envid))
                _environmentId = configuration.PublisherSettings[SqsConfigurationKeys.Envid];


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
            Guard.Against.Null(configuration.PublisherSettings, nameof(configuration.PublisherSettings));

            foreach (var key in ConfigurationKeys.PublisherRequiredKeys)
            {
                if (!configuration.PublisherSettings.ContainsKey(key))
                {
                    throw new ConfigurationException($"{Name} is missing configuration for {key}.");
                }
            }
        }

        public Task Publish(string topic, Monai.Deploy.Messaging.Messages.Message message)
        {

            Guard.Against.NullOrWhiteSpace(topic, nameof(topic));
            Guard.Against.Null(message, nameof(message));


            using var loggerScope = _logger.BeginScope(string.Format(CultureInfo.InvariantCulture, Log.LoggingScopeMessageApplication, message.MessageId, message.ApplicationId));
            _logger.PublishingToSQS(topic, message.MessageId);
            var sendMessageRequest = new SendMessageRequest();

            Dictionary<string, MessageAttributeValue> MessageAttributes = new Dictionary<string, MessageAttributeValue>();
            MessageAttributeValue messageIdAttribute = new MessageAttributeValue();
            messageIdAttribute.DataType = "String";
            messageIdAttribute.StringValue = message.MessageId;
            MessageAttributes.Add("MessageId", messageIdAttribute);

            MessageAttributeValue ContentTypeAttribute = new MessageAttributeValue();
            ContentTypeAttribute.DataType = "String";
            ContentTypeAttribute.StringValue = message.ContentType;
            MessageAttributes.Add("ContentType", ContentTypeAttribute);


            MessageAttributeValue ApplicationIdAttribute = new MessageAttributeValue();
            ApplicationIdAttribute.DataType = "String";
            ApplicationIdAttribute.StringValue = message.MessageId;
            MessageAttributes.Add("ApplicationId", ApplicationIdAttribute);

            sendMessageRequest.MessageAttributes = MessageAttributes;


            Console.WriteLine("Message information : ");
            Console.WriteLine(message);
            Console.WriteLine(message.Body);
            Console.WriteLine(message.Body.Length);


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

            sendMessageRequest.QueueUrl = createQueueResponse.QueueUrl;



            sendMessageRequest.MessageBody = Encoding.UTF8.GetString(message.Body, 0, message.Body.Length);

            try
            {
                _sqSExtendedClient.SendMessageAsync(sendMessageRequest).Wait();
            }
            catch (Exception e)
            {
                _logger.LogError($"The message could not be posted to the queue {queueName} : \n {e.Message}");
            }


            return Task.CompletedTask;
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
    }
}
