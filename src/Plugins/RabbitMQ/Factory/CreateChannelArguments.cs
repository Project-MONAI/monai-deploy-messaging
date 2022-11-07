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

using Ardalis.GuardClauses;

namespace Monai.Deploy.Messaging.RabbitMQ
{
    public class CreateChannelArguments
    {
        public CreateChannelArguments(
            string hostName,
            string password,
            string username,
            string virtualHost,
            string useSSL,
            string portNumber)
        {
            Guard.Against.NullOrWhiteSpace(hostName);
            Guard.Against.NullOrWhiteSpace(password);
            Guard.Against.NullOrWhiteSpace(username);
            Guard.Against.NullOrWhiteSpace(virtualHost);

            HostName = hostName;
            Password = password;
            Username = username;
            VirtualHost = virtualHost;
            UseSSL = useSSL;
            PortNumber = portNumber;
        }

        public string HostName { get; set; }

        public string Password { get; set; }

        public string Username { get; set; }

        public string VirtualHost { get; set; }

        public string UseSSL { get; set; }

        public string PortNumber { get; set; }
    }
}
