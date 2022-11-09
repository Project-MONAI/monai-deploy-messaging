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
        private static readonly ConcurrentDictionary<string, Lazy<ConnectionFactory>> ConnectionFactoriess = new();
        private static readonly ConcurrentDictionary<string, Lazy<IConnection>> Connections = new();
        private static readonly ConcurrentDictionary<string, Lazy<IModel>> Models = new();

        private readonly ILogger<RabbitMQConnectionFactory> _logger;
        private bool _disposedValue;

        public RabbitMQConnectionFactory(ILogger<RabbitMQConnectionFactory> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            if (ConnectionIsOpen(key))
            {
                Models.TryGetValue(key, out var value);
                return value!.Value;
            }

            var connection = Connections.AddOrUpdate(key,
                x => CreatConnection(hostName, username, password, virtualHost, key, useSSL, portNumber),
                (updateKey, updateConnection) =>
                {
                    // If connection to RMQ is lost and:
                    //   - RMQ service returns before calling the next line, then IsOpen returns false
                    //   - a call is made before RMQ returns, then a new connection
                    //      is made with error with IsValueFaulted = true && IsValueCreated = false
                    if (updateConnection.IsValueCreated)
                    {
                        return updateConnection;
                    }
                    else
                    {
                        return CreatConnection(hostName, username, password, virtualHost, key, useSSL, portNumber);
                    }
                });

            var argsObj = new CreateChannelArguments(hostName, password, username, virtualHost, useSSL, portNumber);
            connection.Value.ConnectionShutdown += (connection, args) => OnShutdown(args, key, argsObj);
            connection.Value.CallbackException += (connection, args) => OnException(args, key, argsObj);

            var model = Models.AddOrUpdate(key,
                x =>
                {
                    var model = CreateModelAndAttachEvents(key, connection, argsObj);
                    return new Lazy<IModel>(model);
                },
                (updateKey, updateModel) =>
                {
                    // If connection to RMQ is lost and:
                    //   - RMQ service returns before calling the next line, then IsOpen returns false
                    //   - a call is made before RMQ returns, then a new connection
                    //      is made with error with IsValueFaulted = true && IsValueCreated = false
                    if (updateModel.IsValueCreated)
                    {
                        return updateModel;
                    }
                    else
                    {
                        var model = CreateModelAndAttachEvents(key, connection, argsObj);
                        return new Lazy<IModel>(model);
                    }
                });

            return model.Value;
        }

        private IModel CreateModelAndAttachEvents(string key, Lazy<IConnection> connection, CreateChannelArguments argsObj)
        {
            var model = connection.Value.CreateModel();
            model.ModelShutdown += (connection, args) => OnShutdown(args, key, argsObj);
            model.CallbackException += (connection, args) => OnException(args, key, argsObj);
            return model;
        }

        private static Lazy<IConnection> CreatConnection(string hostName, string username, string password, string virtualHost, string key, string useSSL, string portNumber)
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

            var connectionFactory = ConnectionFactoriess.GetOrAdd(key, y => new Lazy<ConnectionFactory>(() => new ConnectionFactory()
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

        private void OnShutdown(ShutdownEventArgs args, string key, CreateChannelArguments createChannelArguments)
        {
            _logger.ConnectionShutdown(args.ReplyText);

            if (ConnectionIsOpen(key))
            {
                return;
            }

            _logger.ConnectionReconnect();
            Connections.TryRemove(key, out var value);

            if (value is not null)
            {
                value?.Value.Dispose();
            }

            CreateChannel(createChannelArguments);
        }

        private void OnException(CallbackExceptionEventArgs args, string key, CreateChannelArguments createChannelArguments)
        {
            _logger.ConnectionException(args.Exception);

            if (ConnectionIsOpen(key))
            {
                return;
            }

            _logger.ConnectionReconnect();
            CreateChannel(createChannelArguments);
        }

        private static bool ConnectionIsOpen(string key)
        {
            Models.TryGetValue(key, out var model);
            Connections.TryGetValue(key, out var connection);

            if (model is null || connection is null)
            {
                return false;
            }

            if (model.IsValueCreated == false || connection.IsValueCreated == false)
            {
                return false;
            }

            if (connection.Value.IsOpen == false)
            {
                RemoveConnection(key);
                RemoveModel(key);
                return false;
            }

            if (model.Value.IsOpen == false)
            {
                RemoveModel(key);
                return false;
            }

            return true;
        }

        private static void RemoveConnection(string key)
        {
            Connections.TryRemove(key, out var conn);
            if (conn is not null)
            {
                conn.Value.Dispose();
            }
        }

        private static void RemoveModel(string key)
        {
            Models.TryRemove(key, out var mod);
            if (mod is not null)
            {
                mod.Value.Dispose();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _logger.ClosingConnections();
                    foreach (var connection in Connections.Values)
                    {
                        connection.Value.Close();
                    }
                    Connections.Clear();
                    ConnectionFactoriess.Clear();
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
