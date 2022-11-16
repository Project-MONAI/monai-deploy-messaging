/*
 * Copyright 2022 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using RabbitMQ.Client;

namespace Monai.Deploy.Messaging.RabbitMQ
{
    public interface IRabbitMQConnectionFactory
    {
        /// <summary>
        /// Creates a new channel for RabbitMQ client.
        /// The connection factory maintains a single connection to the specified
        /// <c>hostName</c>, <c>username</c>, <c>password</c>, and <c>virtualHost</c> combination.
        /// </summary>
        /// <param name="hostName">Host name</param>
        /// <param name="username">User name</param>
        /// <param name="password">Password</param>
        /// <param name="virtualHost">Virtual host</param>
        /// <param name="useSSL">Encrypt communication</param>
        /// <param name="portNumber">Port Number</param>
        /// <returns>Instance of <see cref="IModel"/>.</returns>
        IModel? CreateChannel( ChannelType type, string hostName, string username, string password, string virtualHost, string useSSL, string portNumber);
    }
}
