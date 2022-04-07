// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using RabbitMQ.Client;

namespace Monai.Deploy.Messaging.RabbitMq
{
    public interface IRabbitMqConnectionFactory
    {
        /// <summary>
        /// Creates a new connection for RabbitMQ client.
        /// </summary>
        /// <param name="hostName">Host name</param>
        /// <param name="username">User name</param>
        /// <param name="password">Password</param>
        /// <param name="virtualHost">Virtual host</param>
        /// <returns>Instance of IConnection.</returns>
        IConnection CreateConnection(string hostName, string username, string password, string virtualHost);
    }

    public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
    {
        private ConnectionFactory? _connectionFactory;

        public IConnection CreateConnection(string hostName, string username, string password, string virtualHost)
        {
            Guard.Against.NullOrWhiteSpace(hostName, nameof(hostName));
            Guard.Against.NullOrWhiteSpace(username, nameof(username));
            Guard.Against.NullOrWhiteSpace(password, nameof(password));
            Guard.Against.NullOrWhiteSpace(virtualHost, nameof(virtualHost));

            if (_connectionFactory is null)
            {
                _connectionFactory = new ConnectionFactory()
                {
                    HostName = hostName,
                    UserName = username,
                    Password = password,
                    VirtualHost = virtualHost
                };
            }

            return _connectionFactory.CreateConnection();
        }
    }
}
