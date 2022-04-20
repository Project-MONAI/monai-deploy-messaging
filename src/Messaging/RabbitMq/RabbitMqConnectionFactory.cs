// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Security.Cryptography;
using System.Text;
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
        /// <param name="createNewConnection">When <c>true</c>, use a cached connection if available. Otherwise, create a new connection.</param>
        /// <returns>Instance of IConnection.</returns>
        IConnection CreateConnection(string hostName, string username, string password, string virtualHost, bool createNewConnection = false);
    }

    public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
    {
        private ConnectionFactory? _connectionFactory;

        private readonly IDictionary<string, IConnection> _connections;

        public RabbitMqConnectionFactory()
        {
            _connections = new Dictionary<string, IConnection>();
        }

        public IConnection CreateConnection(string hostName, string username, string password, string virtualHost, bool createNewConnection = false)
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
            var key = $"{hostName}{username}{HashPassword(password)}{virtualHost}";
            if (createNewConnection || !_connections.ContainsKey(key) || !_connections[key].IsOpen)
            {
                var connection = _connectionFactory.CreateConnection();
                _connections[key] = connection;
                return connection;
            }
            else
            {
                return _connections[key];
            }
        }

        private object HashPassword(string password)
        {
            Guard.Against.NullOrWhiteSpace(password, nameof(password));
            var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return hash.Select(x => x.ToString("x2"));
        }
    }
}
