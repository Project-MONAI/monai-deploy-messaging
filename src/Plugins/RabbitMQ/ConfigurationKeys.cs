// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.Messaging.RabbitMQ
{
    internal static class ConfigurationKeys
    {
        public static readonly string EndPoint = "endpoint";
        public static readonly string Username = "username";
        public static readonly string Password = "password";
        public static readonly string VirtualHost = "virtualHost";
        public static readonly string Exchange = "exchange";
        public static readonly string DeadLetterExchange = "deadLetterExchange";
        public static readonly string DeliveryLimit = "deliveryLimit";
        public static readonly string RequeueDelay = "requeueDelay";
        public static readonly string UseSSL = "useSSL";
        public static readonly string Port = "port";
        public static readonly string[] PublisherRequiredKeys = new[] { EndPoint, Username, Password, VirtualHost, Exchange };
        public static readonly string[] SubscriberRequiredKeys = new[] { EndPoint, Username, Password, VirtualHost, Exchange, DeadLetterExchange, DeliveryLimit, RequeueDelay };
    }
}
