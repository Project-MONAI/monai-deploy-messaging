/*
 * Copyright 2021-2024 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Monai.Deploy.Messaging.RabbitMQ
{
    internal static class ConfigurationKeys
    {
        public static readonly string PublisherServiceName = "Rabbit MQ Publisher";
        public static readonly string SubscriberServiceName = "Rabbit MQ Subscriber";

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
        public static readonly string[] PublisherRequiredKeys = [EndPoint, Username, Password, VirtualHost, Exchange];
        public static readonly string[] SubscriberRequiredKeys = [EndPoint, Username, Password, VirtualHost, Exchange, DeadLetterExchange, DeliveryLimit, RequeueDelay];
        public static readonly string PrefetchCount = "prefetchCount";
    }
}
