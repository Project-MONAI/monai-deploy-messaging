// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.DependencyInjection;

namespace Monai.Deploy.Messaging.RabbitMQ
{
    public class PublisherServiceRegistration : PublisherServiceRegistrationBase
    {
        public PublisherServiceRegistration(string fullyQualifiedAssemblyName) : base(fullyQualifiedAssemblyName)
        {
        }

        public override IServiceCollection Configure(IServiceCollection services)
        {
            return services
                .AddSingleton<IRabbitMQConnectionFactory, RabbitMQConnectionFactory>()
                .AddSingleton<IMessageBrokerPublisherService, RabbitMQMessagePublisherService>();
        }
    }
}
