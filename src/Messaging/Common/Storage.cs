﻿// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Monai.Deploy.Messaging.Common
{
    public class Storage
    {
        /// <summary>
        /// Gets or sets the endpoint of the storage service.
        /// </summary>
        [JsonProperty(PropertyName = "endpoint")]
        [Required]
        public string? Endpoint { get; set; }

        /// <summary>
        /// Gets or sets credentials for accessing the storage service.
        /// </summary>
        [JsonProperty(PropertyName = "credentials")]
        [Required]
        public Credentials? Credentials { get; set; }

        /// <summary>
        /// Gets or sets name of the bucket.
        /// </summary>
        [JsonProperty(PropertyName = "bucket")]
        [Required]
        public string? Bucket { get; set; }

        /// <summary>
        /// Gets or sets whether the connection should be secured or not.
        /// </summary>
        [JsonProperty(PropertyName = "secured_connection")]
        public bool SecuredConnection { get; set; } = false;

        /// <summary>
        /// Gets or sets the optional relative root path to the data.
        /// </summary>
        [JsonProperty(PropertyName = "relative_root_path")]
        [Required]
        public string? RelativeRootPath { get; set; }
    }
}