// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.ComponentModel.DataAnnotations;
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
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the endpoint of the storage service.
        /// </summary>
        [JsonProperty(PropertyName = "endpoint")]
        [Required]
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets credentials for accessing the storage service.
        /// </summary>
        [JsonProperty(PropertyName = "credentials")]
        public Credentials? Credentials { get; set; }

        /// <summary>
        /// Gets or sets name of the bucket.
        /// </summary>
        [JsonProperty(PropertyName = "bucket")]
        [Required]
        public string Bucket { get; set; }

        /// <summary>
        /// Gets or sets whether the connection should be secured or not.
        /// </summary>
        [JsonProperty(PropertyName = "secured_connection")]
        public bool SecuredConnection { get; set; }

        /// <summary>
        /// Gets or sets the optional relative root path to the data.
        /// </summary>
        [JsonProperty(PropertyName = "relative_root_path")]
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
