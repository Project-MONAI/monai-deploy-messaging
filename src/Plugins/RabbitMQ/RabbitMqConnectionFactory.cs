// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Monai.Deploy.Messaging.RabbitMQ
{
    public interface IRabbitMQConnectionFactory
    {
        /// <summary>
        /// Creates a new channel for RabbitMQ client.
        /// THe connection factory maintains a single connection to the specified
        /// <c>hostName</c>, <c>username</c>, <c>password</c>, and <c>virtualHost</c> combination.
        /// </summary>
        /// <param name="hostName">Host name</param>
        /// <param name="username">User name</param>
        /// <param name="password">Password</param>
        /// <param name="virtualHost">Virtual host</param>
        /// <param name="useSSL">Encrypt communication</param>
        /// <param name="portNumber">Port Number</param>
        /// <returns>Instance of <see cref="IModel"/>.</returns>
        IModel CreateChannel(string hostName, string username, string password, string virtualHost, string useSSL, string portNumber);
    }

    public class RabbitMQConnectionFactory : IRabbitMQConnectionFactory, IDisposable
    {
        private readonly ConcurrentDictionary<string, Lazy<ConnectionFactory>> _connectionFactoriess;
        private readonly ConcurrentDictionary<string, Lazy<IConnection>> _connections;
        private readonly ILogger<RabbitMQConnectionFactory> _logger;
        private bool _disposedValue;

        public RabbitMQConnectionFactory(ILogger<RabbitMQConnectionFactory> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionFactoriess = new ConcurrentDictionary<string, Lazy<ConnectionFactory>>();
            _connections = new ConcurrentDictionary<string, Lazy<IConnection>>();
        }

        public IModel CreateChannel(string hostName, string username, string password, string virtualHost, string useSSL, string portNumber)
        {
            Guard.Against.NullOrWhiteSpace(hostName, nameof(hostName));
            Guard.Against.NullOrWhiteSpace(username, nameof(username));
            Guard.Against.NullOrWhiteSpace(password, nameof(password));
            Guard.Against.NullOrWhiteSpace(virtualHost, nameof(virtualHost));

            var key = $"{hostName}{username}{HashPassword(password)}{virtualHost}";

            var connection = _connections.AddOrUpdate(key,
                x =>
                {
                    return CreatConnection(hostName, username, password, virtualHost, key, useSSL, portNumber);
                },
                (updateKey, updateConnection) =>
                {
                    if (updateConnection.Value.IsOpen)
                    {
                        return updateConnection;
                    }
                    else
                    {
                        return CreatConnection(hostName, username, password, virtualHost, key, useSSL, portNumber);
                    }
                });

            return connection.Value.CreateModel();
        }

        private Lazy<IConnection> CreatConnection(string hostName, string username, string password, string virtualHost, string key, string useSSL, string portNumber)
        {
            if (!bool.TryParse(useSSL, out var sslEnabled))
            {
                sslEnabled = false;
            }

            if (!Int32.TryParse(portNumber, out var port))
            {
                port = sslEnabled ? 5671 : 5672; // 5671 is default port for SSL/TLS , 5672 is default port for PLAIN.
            }

            var sslOptions = new SslOption
            {
                Enabled = sslEnabled,
                ServerName = hostName,
                AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNotAvailable
            };

            var connectionFactory = _connectionFactoriess.GetOrAdd(key, y => new Lazy<ConnectionFactory>(() => new ConnectionFactory()
            {
                HostName = hostName,
                UserName = username,
                Password = password,
                VirtualHost = virtualHost,
                Ssl = sslOptions,
                Port = port
            }));

            return new Lazy<IConnection>(() => connectionFactory.Value.CreateConnection());
        }

        private object HashPassword(string password)
        {
            Guard.Against.NullOrWhiteSpace(password, nameof(password));
            var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return hash.Select(x => x.ToString("x2"));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _logger.ClosingConnections();
                    foreach (var connection in _connections.Values)
                    {
                        connection.Value.Close();
                    }
                    _connections.Clear();
                    _connectionFactoriess.Clear();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
