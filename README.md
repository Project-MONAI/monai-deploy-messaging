<p align="center">
<img src="https://raw.githubusercontent.com/Project-MONAI/MONAI/dev/docs/images/MONAI-logo-color.png" width="50%" alt='project-monai'>
</p>

💡 If you want to know more about MONAI Deploy WG vision, overall structure, and guidelines, please read [MONAI Deploy](https://github.com/Project-MONAI/monai-deploy) first.

# MONAI Deploy Messaging

The MONAI Deploy Messaging library for MONAI Deploy clinical data pipelines system enables users to extend the system to external message broker services by implementing the [IMessageBrokerPublisherService](src/Messaging/API/IMessageBrokerPublisherService.cs) and [IMessageBrokerSubscriberService](src/Messaging/API/IMessageBrokerSubscriberService.cs) APIs. The APIs allow users to plug in any other message broker services, such as [Apache Kafka](https://kafka.apache.org/intro) and [Azure Service Bus](https://azure.microsoft.com/en-us/services/service-bus/).

Currently supported message broker services:

- [RabbitMQ](https://www.rabbitmq.com/)*

\* Services provided may not be free or requires special license agreements. Please refer to the service providers' website for additional terms and conditions.

If you would like to use a message broker service not listed above, please file an [issue](https://github.com/Project-MONAI/monai-deploy-messaging/issues) and contribute to the repository.

## Contributing

For guidance on making a contribution to MONAI Deploy Messaging, see the [contributing guidelines](https://github.com/Project-MONAI/monai-deploy/blob/main/CONTRIBUTING.md).

Join the conversation on Twitter [@ProjectMONAI](https://twitter.com/ProjectMONAI) or join our [Slack channel](https://forms.gle/QTxJq3hFictp31UM9).

Ask and answer questions over on [MONAI Deploy Messaging's GitHub Discussions tab](https://github.com/Project-MONAI/monai-deploy-messaging/discussions).

## Links

- Website: <https://monai.io>
- Code: <https://github.com/Project-MONAI/monai-deploy-messaging>
- Project tracker: <https://github.com/Project-MONAI/monai-deploy-messaging/projects>
- Issue tracker: <https://github.com/Project-MONAI/monai-deploy-messaging/issues>
- Test status: <https://github.com/Project-MONAI/monai-deploy-messaging/actions>
