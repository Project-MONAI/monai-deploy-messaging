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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Monai.Deploy.Messaging.RabbitMQ
{
    public class RabbitMQConnectionFactory : IRabbitMQConnectionFactory, IDisposable
    {
        private readonly ConcurrentDictionary<string, Lazy<ConnectionFactory>> _connectionFactories = new();
        private readonly ConcurrentDictionary<string, (IConnection connection, IModel model)> _connections = new();
        private readonly ILogger<RabbitMQConnectionFactory> _logger;
        private bool _disposedValue;

        internal IReadOnlyList<IConnection> Connections
        {
            get { return _connections.Values.Select(p => p.connection).ToList(); }
        }

        public RabbitMQConnectionFactory(ILogger<RabbitMQConnectionFactory> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IModel CreateChannel(ChannelType type, string hostName, string username, string password, string virtualHost, string useSSL, string portNumber)
        {
            Guard.Against.NullOrWhiteSpace(hostName);
            Guard.Against.NullOrWhiteSpace(username);
            Guard.Against.NullOrWhiteSpace(password);
            Guard.Against.NullOrWhiteSpace(virtualHost);

            var key = $"{type}{hostName}{username}{HashPassword(password)}{virtualHost}";

            var connection = _connections.AddOrUpdate(key,
                x => MakeConnection(type, hostName, username, password, virtualHost, key, useSSL, portNumber),
                (updateKey, updateConnection) =>
                {
                    // If connection to RMQ is lost and:
                    //   - RMQ service returns before calling the next line, then IsOpen returns false
                    //   - a call is made before RMQ returns, then a new connection
                    //      is made with error with IsValueFaulted = true && IsValueCreated = false
                    if (updateConnection.connection is not null && updateConnection.connection.IsOpen)
                    {
                        if (updateConnection.model.IsClosed)
                        {
                            updateConnection.model = MakeChannel(type, updateConnection.connection, key);
                        }
                        return updateConnection;
                    }
                    else
                    {
                        return MakeConnection(type, hostName, username, password, virtualHost, key, useSSL, portNumber);
                    }
                });

            return connection.model;
        }

        private void ConnectionShutdown(ShutdownEventArgs args, IConnection value, string key)
        {
            _logger.ConnectionShutdown(args.ToString());
        }

        private void OnException(CallbackExceptionEventArgs args, IConnection value, string key)
        {
            _logger.ConnectionException(args.Exception);
        }

        private (IConnection, IModel) MakeConnection(ChannelType type, string hostName, string username, string password, string virtualHost, string key, string useSSL, string portNumber)
        {
            var connection = CreateConnectionOnly(hostName, username, password, virtualHost, key, useSSL, portNumber);
            var model = MakeChannel(type, connection, key);
            return (connection, model);
        }

        private IModel MakeChannel(ChannelType type, IConnection connection, string key)
        {
            var model = connection.CreateModel();
            model.CallbackException += (sender, args) => OnException(args, connection, key);
            model.ModelShutdown += (sender, args) => ConnectionShutdown(args, connection, key);
            if (type == ChannelType.Publisher)
            {
                model.ConfirmSelect();
            }
            return model;
        }

        private IConnection CreateConnectionOnly(string hostName, string username, string password, string virtualHost, string key, string useSSL, string portNumber)
        {
            if (!bool.TryParse(useSSL, out var sslEnabled))
            {
                sslEnabled = false;
            }

            if (!int.TryParse(portNumber, out var port))
            {
                port = sslEnabled ? 5671 : 5672; // 5671 is default port for SSL/TLS , 5672 is default port for PLAIN.
            }

            var sslOptions = new SslOption
            {
                Enabled = sslEnabled,
                ServerName = hostName,
                AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNotAvailable
            };

            var connectionFactory = _connectionFactories.GetOrAdd(key, y => new Lazy<ConnectionFactory>(() => new ConnectionFactory()
            {
                HostName = hostName,
                UserName = username,
                Password = password,
                VirtualHost = virtualHost,
                Ssl = sslOptions,
                Port = port,
                RequestedHeartbeat = TimeSpan.FromSeconds(10),
                AutomaticRecoveryEnabled = true
            }));

            var connection = connectionFactory.Value.CreateConnection();
            connection.ConnectionShutdown += (sender, args) => ConnectionShutdown(args, connection, key);
            connection.CallbackException += (sender, args) => OnException(args, connection, key);
            return connection;
        }

        private static object HashPassword(string password)
        {
            Guard.Against.NullOrWhiteSpace(password);
            var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return string.Join("", hash.Select(x => x.ToString("x2", CultureInfo.InvariantCulture)));
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
                        connection.connection.Close();
                    }
                    _connections.Clear();
                    _connectionFactories.Clear();
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
