# RabbitMQ for MONAI Deploy

## Overview

The RabbitMQ plug-in for MONAI Deploy is based on the [RabbitMQ](https://www.rabbitmq.com/) solution.

## Configuration


The following configurations are required to run the MinIO plug-in.


### Default Configurations for the Message Publisher

The `publisherServiceAssemblyName` should be set to `Monai.Deploy.Messaging.RabbitMQ.RabbitMQMessagePublisherService, Monai.Deploy.Messaging.RabbitMQ`.

The following configurations are required for the publisher service.

| Key         | Description                                                                      | Sample Value |
| ----------- | -------------------------------------------------------------------------------- | ------------ |
| endpoint    | Host name/IP and port.                                                           | localhost    |
| username    | Username                                                                         | username     |
| password    | Password                                                                         | password     |
| virtualHost | Name of the virtual host                                                         | monaideploy  |
| exchange    | Name of the exchange                                                             | monaideploy  |
| useSSL      | (optional) use secured connection or not                                         | false        |
| port        | (optional) port number with default of 5672 for plaint-text and 5671 for secured | 5672         |


### Default Configurations for the Message Subscriber

The `subscriberServiceAssemblyName` should be set to `Monai.Deploy.Messaging.RabbitMQ.RabbitMQMessageSubscriberService, Monai.Deploy.Messaging.RabbitMQ`.

The following configurations are required for the subscriber service.

| Key         | Description                                                                      | Sample Value |
| ----------- | -------------------------------------------------------------------------------- | ------------ |
| endpoint    | Host name/IP and port.                                                           | localhost    |
| username    | Username                                                                         | username     |
| password    | Password                                                                         | password     |
| virtualHost | Name of the virtual host                                                         | monaideploy  |
| exchange    | Name of the exchange                                                             | monaideploy  |
| useSSL      | (optional) use secured connection or not                                         | false        |
| port        | (optional) port number with default of 5672 for plaint-text and 5671 for secured | 5672         |
