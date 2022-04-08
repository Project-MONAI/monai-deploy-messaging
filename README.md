<p align="center">
<img src="https://raw.githubusercontent.com/Project-MONAI/MONAI/dev/docs/images/MONAI-logo-color.png" width="50%" alt='project-monai'>
</p>

💡 If you want to know more about MONAI Deploy WG vision, overall structure, and guidelines, please read [MONAI Deploy](https://github.com/Project-MONAI/monai-deploy) first.

# MONAI Deploy Messaging

[![License](https://img.shields.io/badge/license-Apache%202.0-green.svg)](LICENSE)
[![codecov](https://codecov.io/gh/Project-MONAI/monai-deploy-messaging/branch/master/graph/badge.svg?token=a7lu3x6kEo)](https://codecov.io/gh/Project-MONAI/monai-deploy-messaging)
[![ci](https://github.com/Project-MONAI/monai-deploy-messaging/actions/workflows/ci.yml/badge.svg)](https://github.com/Project-MONAI/monai-deploy-messaging/actions/workflows/ci.yml)
[![Nuget](https://img.shields.io/nuget/dt/Monai.Deploy.Messaging?label=NuGet%20Download)](https://www.nuget.org/packages/Monai.Deploy.Messaging/)

The MONAI Deploy Messaging library for MONAI Deploy clinical data pipelines system enables users to extend the system to external message broker services by implementing the [IMessageBrokerPublisherService](src/Messaging/API/IMessageBrokerPublisherService.cs) and [IMessageBrokerSubscriberService](src/Messaging/API/IMessageBrokerSubscriberService.cs) APIs. The APIs allow users to plug in any other message broker services, such as [Apache Kafka](https://kafka.apache.org/intro) and [Azure Service Bus](https://azure.microsoft.com/en-us/services/service-bus/).

Currently supported message broker services:

- [RabbitMQ](https://www.rabbitmq.com/)*

\* Services provided may not be free or requires special license agreements. Please refer to the service providers' website for additional terms and conditions.

If you would like to use a message broker service not listed above, please file an [issue](https://github.com/Project-MONAI/monai-deploy-messaging/issues) and contribute to the repository.

## Releases

The MONAI Deploy Storage library is release in NuGet format which is available on both [NuGet.Org](https://www.nuget.org/packages/Monai.Deploy.Storage/) and [GitHub](https://github.com/Project-MONAI/monai-deploy-storage/packages/1350678).

### Official Builds

Official builds are made from the `main` branch.

### RC Builds

Release candidates are built and released from the `release/*` branches.

### Development Builds

Development builds are made from all branches except the `main` branch and the `release/*` branches.  The NuGet packages are released to [GitHub](https://github.com/Project-MONAI/monai-deploy-storage/packages/1350678) only.

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
