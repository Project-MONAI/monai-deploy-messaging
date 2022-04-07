// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Microsoft.Extensions.DependencyInjection;

namespace Monai.Deploy.Messaging.RabbitMq
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection UseRabbitMq(this IServiceCollection services)
        {
            services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();

            return services;
        }
    }
}
