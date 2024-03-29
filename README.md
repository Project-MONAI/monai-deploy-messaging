
<p align="center">
<img src="https://raw.githubusercontent.com/Project-MONAI/MONAI/dev/docs/images/MONAI-logo-color.png" width="50%" alt='project-monai'>
</p>

💡 If you want to know more about MONAI Deploy WG vision, overall structure, and guidelines, please read [MONAI Deploy](https://github.com/Project-MONAI/monai-deploy) first.

# MONAI Deploy Messaging

[![License](https://img.shields.io/badge/license-Apache%202.0-green.svg)](LICENSE)
[![codecov](https://codecov.io/gh/Project-MONAI/monai-deploy-messaging/branch/master/graph/badge.svg?token=a7lu3x6kEo)](https://codecov.io/gh/Project-MONAI/monai-deploy-messaging)
[![ci](https://github.com/Project-MONAI/monai-deploy-messaging/actions/workflows/ci.yml/badge.svg)](https://github.com/Project-MONAI/monai-deploy-messaging/actions/workflows/ci.yml)
[![Nuget](https://img.shields.io/nuget/dt/Monai.Deploy.Messaging?label=NuGet%20Download)](https://www.nuget.org/packages/Monai.Deploy.Messaging/)

The MONAI Deploy Messaging library for MONAI Deploy clinical data pipelines system enables users to extend the system to external message broker services by implementing the [IMessageBrokerPublisherService](src/Messaging/API/IMessageBrokerPublisherService.cs) and [IMessageBrokerSubscriberService](src/Messaging/API/IMessageBrokerSubscriberService.cs) APIs. The APIs allow the users to plug in any other message broker services, such as [Apache Kafka](https://kafka.apache.org/intro) and [Azure Service Bus](https://azure.microsoft.com/en-us/services/service-bus/).

Currently supported message broker services:

- [RabbitMQ](https://www.rabbitmq.com/)*

\* Services provided may not be free or requires special license agreements. Please refer to the service providers' website for additional terms and conditions.

If you would like to use a message broker service not listed above, please file an [issue](https://github.com/Project-MONAI/monai-deploy-messaging/issues) and contribute to the repository.

---

## Installation

### 1. Configure the Service
To use the MONAI Deploy Messaging library, install the [NuGet.Org](https://www.nuget.org/packages/Monai.Deploy.Messaging/) package and call the `AddMonaiDeployMessageBrokerSubscriberService(...)` and/or the `AddMonaiDeployMessageBrokerPublisherService(...)` method to register the dependencies:

```csharp
Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        ...
        // Register the subscriber service
        services.AddMonaiDeployMessageBrokerSubscriberService(hostContext.Configuration.GetSection("InformaticsGateway:messaging:publisherServiceAssemblyName").Value);

        // Register the publisher service
        services.AddMonaiDeployMessageBrokerPublisherService(hostContext.Configuration.GetSection("InformaticsGateway:messaging:subscriberServiceAssemblyName").Value);
        ...
    });
```

### 2. Install the Plug-in

1. Create a subdirectory named `plug-ins` in the directory where your main application is installed.
2. Download the zipped plug-in of your choice and extract the files to the `plug-ins` directory.
3. Update `appsettings.json` and set the `publisherServiceAssemblyName` and the `subscriberServiceAssemblyName`, e.g.:  
   ```json
    "messaging": {
      "publisherServiceAssemblyName": "Monai.Deploy.Messaging.RabbitMQ.RabbitMQMessagePublisherService, Monai.Deploy.Messaging.RabbitMQ",
      "publisherSettings": {
        ...
      },
      "subscriberServiceAssemblyName": "Monai.Deploy.Messaging.RabbitMQ.RabbitMQMessageSubscriberService, Monai.Deploy.Messaging.RabbitMQ",
      "subscriberSettings": {
        ...
      }
    },
   ```


### 3. Restrict Acess to the Plug-ins Directory

To avoid tampering of the plug-ins, it is recommended to set access rights to the plug-ins directory.

---

## Releases

The MONAI Deploy Messaging library is released in NuGet format, which is available on both [NuGet.Org](https://www.nuget.org/packages/Monai.Deploy.Messaging/) and [GitHub](https://github.com/Project-MONAI/monai-deploy-messaging/packages/1365839).

### Official Builds

Official builds are made from the `main` branch.

### RC Builds

Release candidates are built and released from the `release/*` branches.

### Development Builds

Development builds are made from all branches except the `main` branch and the `release/*` branches. The NuGet packages are released to [GitHub](https://github.com/Project-MONAI/monai-deploy-messaging/packages/1365839) only.

## Contributing

For guidance on contributing to MONAI Deploy Messaging, see the [contributing guidelines](https://github.com/Project-MONAI/monai-deploy/blob/main/CONTRIBUTING.md).

Join the conversation on Twitter [@ProjectMONAI](https://twitter.com/ProjectMONAI) or join our [Slack channel](https://forms.gle/QTxJq3hFictp31UM9).

Ask and answer questions over on [MONAI Deploy Messaging's GitHub Discussions tab](https://github.com/Project-MONAI/monai-deploy-messaging/discussions).

## License

Copyright (c) MONAI Consortium. All rights reserved.
Licensed under the [Apache-2.0](LICENSE) license.

This software uses the Microsoft .NET 6.0 library, and the use of this software is subject to the [Microsoft software license terms](https://dotnet.microsoft.com/en-us/dotnet_library_license.htm).

By downloading this software, you agree to the license terms & all licenses listed on the [third-party licenses](third-party-licenses.md) page.

## Links

- Website: <https://monai.io>
- Code: <https://github.com/Project-MONAI/monai-deploy-messaging>
- Project tracker: <https://github.com/Project-MONAI/monai-deploy-messaging/projects>
- Issue tracker: <https://github.com/Project-MONAI/monai-deploy-messaging/issues>
- Test status: <https://github.com/Project-MONAI/monai-deploy-messaging/actions>
