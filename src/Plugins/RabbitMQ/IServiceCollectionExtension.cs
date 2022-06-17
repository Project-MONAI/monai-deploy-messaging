// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.DependencyInjection;

namespace Monai.Deploy.Messaging.RabbitMQ
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection UseRabbitMQ(this IServiceCollection services)
        {
            services.AddSingleton<IRabbitMQConnectionFactory, RabbitMQConnectionFactory>();

            return services;
        }
    }
}
