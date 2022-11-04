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

        public IModel CreateChannel(CreateChannelArguments args) =>
            CreateChannel(args.HostName, args.Username, args.Password, args.VirtualHost,
                          args.UseSSL, args.PortNumber);

        public IModel CreateChannel(string hostName, string username, string password, string virtualHost, string useSSL, string portNumber)
        {
            Guard.Against.NullOrWhiteSpace(hostName);
            Guard.Against.NullOrWhiteSpace(username);
            Guard.Against.NullOrWhiteSpace(password);
            Guard.Against.NullOrWhiteSpace(virtualHost);

            var key = $"{hostName}{username}{HashPassword(password)}{virtualHost}";

            var connection = _connections.AddOrUpdate(key,
                x => CreatConnection(hostName, username, password, virtualHost, key, useSSL, portNumber),
                (updateKey, updateConnection) =>
                {
                    // If connection to RMQ is lost and:
                    //   - RMQ service returns before calling the next line, then IsOpen returns false
                    //   - a call is made before RMQ returns, then a new connection
                    //      is made with error with IsValueFaulted = true && IsValueCreated = false
                    if (updateConnection.IsValueCreated && updateConnection.Value.IsOpen)
                    {
                        return updateConnection;
                    }
                    else
                    {
                        return CreatConnection(hostName, username, password, virtualHost, key, useSSL, portNumber);
                    }
                });

            var model = connection.Value.CreateModel();

            var argsObj = new CreateChannelArguments(hostName, password, username, virtualHost, useSSL, portNumber);

            model.CallbackException += (connection, args) => OnException(args, key, argsObj);
            model.ModelShutdown += (connection, args) => OnShutdown(args, key, argsObj);

            return model;
        }

        private void OnShutdown(ShutdownEventArgs args, string key, CreateChannelArguments createChannelArguments)
        {
            _logger.LogError($"RabbitMQ connection shutdown attempting to reconnect.", args);
            _connections.TryRemove(key, out var value);

            if (value is not null)
            {
                value?.Value.Dispose();
            }

            CreateChannel(createChannelArguments);
        }

        private void OnException(CallbackExceptionEventArgs args, string key, CreateChannelArguments createChannelArguments)
        {
            _logger.LogError(args.Exception, $"RabbitMQ connection exception attempting to reconnect {args.Exception.Message}");
            _connections.TryRemove(key, out var value);

            if (value is not null)
            {
                value?.Value.Dispose();
            }

            CreateChannel(createChannelArguments);
        }

        private Lazy<IConnection> CreatConnection(string hostName, string username, string password, string virtualHost, string key, string useSSL, string portNumber)
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

            var connectionFactory = _connectionFactoriess.GetOrAdd(key, y => new Lazy<ConnectionFactory>(() => new ConnectionFactory()
            {
                HostName = hostName,
                UserName = username,
                Password = password,
                VirtualHost = virtualHost,
                Ssl = sslOptions,
                Port = port,
                RequestedHeartbeat = TimeSpan.FromSeconds(10),
            }));

            return new Lazy<IConnection>(connectionFactory.Value.CreateConnection);
        }

        private static object HashPassword(string password)
        {
            Guard.Against.NullOrWhiteSpace(password);
            var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return hash.Select(x => x.ToString("x2", CultureInfo.InvariantCulture));
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
