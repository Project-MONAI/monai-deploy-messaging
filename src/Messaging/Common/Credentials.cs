// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Monai.Deploy.Messaging.Common
{
    public class Credentials
    {
        /// <summary>
        /// Access key or username of the credentials pair.
        /// </summary>
        [JsonProperty(PropertyName = "access_key")]
        [Required]
        public string? AccessKey { get; set; }

        /// <summary>
        /// Access token or password of the credentials pair.
        /// </summary>
        [JsonProperty(PropertyName = "access_token")]
        [Required]
        public string? AccessToken { get; set; }

        /// <summary>
        /// Session token of the credentials pair.
        /// </summary>
        [JsonProperty(PropertyName = "session_token")]
        public string? SessionToken { get; set; }

    }
}
