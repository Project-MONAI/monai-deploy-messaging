/*
 * Copyright 2021-2025 MONAI Consortium
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
    public class Credentials
    {
        /// <summary>
        /// Gets or sets the access key or user name of the credentials pair.
        /// </summary>
        [JsonProperty(PropertyName = "access_key")]
        [JsonPropertyName("access_key")]
        [Required]
        public string AccessKey { get; set; }

        /// <summary>
        /// Gets or sets the access token or password of the credentials pair.
        /// </summary>
        [JsonProperty(PropertyName = "access_token")]
        [JsonPropertyName("access_token")]
        [Required]
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the session token of the credentials pair.
        /// </summary>
        [JsonProperty(PropertyName = "session_token")]
        [JsonPropertyName("session_token")]
        public string SessionToken { get; set; }

        public Credentials()
        {
            AccessKey = string.Empty;
            AccessToken = string.Empty;
            SessionToken = string.Empty;
        }
    }
}
