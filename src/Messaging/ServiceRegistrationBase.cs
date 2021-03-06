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
using Microsoft.Extensions.DependencyInjection;
using Monai.Deploy.Messaging.Configuration;

namespace Monai.Deploy.Messaging
{
    public abstract class SubscriberServiceRegistrationBase : ServiceRegistrationBase
    {
        protected SubscriberServiceRegistrationBase(string fullyQualifiedAssemblyName) : base(fullyQualifiedAssemblyName)
        {
        }
    }

    public abstract class PublisherServiceRegistrationBase : ServiceRegistrationBase
    {
        protected PublisherServiceRegistrationBase(string fullyQualifiedAssemblyName) : base(fullyQualifiedAssemblyName)
        {
        }
    }

    public abstract class ServiceRegistrationBase
    {
        protected string FullyQualifiedAssemblyName { get; }
        protected string AssemblyFilename { get; }

        protected ServiceRegistrationBase(string fullyQualifiedAssemblyName)
        {
            Guard.Against.NullOrWhiteSpace(fullyQualifiedAssemblyName, nameof(fullyQualifiedAssemblyName));
            FullyQualifiedAssemblyName = fullyQualifiedAssemblyName;
            AssemblyFilename = ParseAssemblyName();
        }

        private string ParseAssemblyName()
        {
            var assemblyNameParts = FullyQualifiedAssemblyName.Split(',', StringSplitOptions.None);
            if (assemblyNameParts.Length < 2 || string.IsNullOrWhiteSpace(assemblyNameParts[1]))
            {
                throw new ConfigurationException($"Storage service '{FullyQualifiedAssemblyName}' is invalid.  Please provide a fully qualified name.")
                {
                    HelpLink = "https://docs.microsoft.com/en-us/dotnet/standard/assembly/find-fully-qualified-name"
                };
            }

            return assemblyNameParts[1].Trim();
        }

        public abstract IServiceCollection Configure(IServiceCollection services);
    }
}
