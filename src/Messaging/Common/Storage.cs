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

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Monai.Deploy.Messaging.Common
{
    public class Storage : ICloneable
    {
        /// <summary>
        /// Gets or sets the name of the artifact.
        /// For Argo, name of the artifact used in the template.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        [JsonPropertyName("name")]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the endpoint of the storage service.
        /// </summary>
        [JsonProperty(PropertyName = "endpoint")]
        [JsonPropertyName("endpoint")]
        [Required]
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets credentials for accessing the storage service.
        /// </summary>
        [JsonProperty(PropertyName = "credentials")]
        [JsonPropertyName("credentials")]
        public Credentials? Credentials { get; set; }

        /// <summary>
        /// Gets or sets name of the bucket.
        /// </summary>
        [JsonProperty(PropertyName = "bucket")]
        [JsonPropertyName("bucket")]
        [Required]
        public string Bucket { get; set; }

        /// <summary>
        /// Gets or sets whether the connection should be secured or not.
        /// </summary>
        [JsonProperty(PropertyName = "secured_connection")]
        [JsonPropertyName("secured_connection")]
        public bool SecuredConnection { get; set; }

        /// <summary>
        /// Gets or sets the optional relative root path to the data.
        /// </summary>
        [JsonProperty(PropertyName = "relative_root_path")]
        [JsonPropertyName("relative_root_path")]
        [Required]
        public string RelativeRootPath { get; set; }

        public Storage()
        {
            Name = string.Empty;
            Endpoint = string.Empty;
            Credentials = null;
            Bucket = string.Empty;
            SecuredConnection = false;
            RelativeRootPath = string.Empty;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
