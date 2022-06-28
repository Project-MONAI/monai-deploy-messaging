// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.DependencyInjection;
using Monai.Deploy.Messaging.API;

namespace Monai.Deploy.Messaging.RabbitMQ
{
    public class SubscriberServiceRegistration : SubscriberServiceRegistrationBase
    {
        public SubscriberServiceRegistration(string fullyQualifiedAssemblyName) : base(fullyQualifiedAssemblyName)
        {
        }

        public override IServiceCollection Configure(IServiceCollection services)
        {
            services.AddSingleton<IRabbitMQConnectionFactory, RabbitMQConnectionFactory>();
            services.AddSingleton<IMessageBrokerSubscriberService, RabbitMQMessageSubscriberService>();
            return services;
        }
    }
}
