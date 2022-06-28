// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Monai.Deploy.Messaging.Common
{
    public class Credentials
    {
        /// <summary>
        /// Gets or sets the access key or user name of the credentials pair.
        /// </summary>
        [JsonProperty(PropertyName = "access_key")]
        [Required]
        public string AccessKey { get; set; }

        /// <summary>
        /// Gets or sets the access token or password of the credentials pair.
        /// </summary>
        [JsonProperty(PropertyName = "access_token")]
        [Required]
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the session token of the credentials pair.
        /// </summary>
        [JsonProperty(PropertyName = "session_token")]
        public string SessionToken { get; set; }

        public Credentials()
        {
            AccessKey = string.Empty;
            AccessToken = string.Empty;
            SessionToken = string.Empty;
        }
    }
}
