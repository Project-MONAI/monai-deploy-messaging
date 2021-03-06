/*
 * Copyright 2021-2022 MONAI Consortium
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
 
using System.ComponentModel.DataAnnotations;
using Monai.Deploy.Messaging.Common;
using Newtonsoft.Json;

namespace Monai.Deploy.Messaging.Events
{
    public class WorkflowRequestEvent : EventBase
    {
        private readonly List<BlockStorageInfo> _payload;

        /// <summary>
        /// Gets or sets the ID of the payload which is also used as the root path of the payload.
        /// </summary>
        [JsonProperty(PropertyName = "payload_id")]
        [Required]
        public Guid PayloadId { get; set; }

        /// <summary>
        /// Gets or sets the associated workflows to be launched.
        /// </summary>
        [JsonProperty(PropertyName = "workflows")]
        public IEnumerable<string> Workflows { get; set; }

        /// <summary>
        /// Gets or sets number of files in the payload.
        /// </summary>
        [JsonProperty(PropertyName = "file_count")]
        [Required]
        public int FileCount { get; set; }

        /// <summary>
        /// For DIMSE, the correlation ID is the UUID associated with the first DICOM association received.
        /// For an ACR inference request, the correlation ID is the Transaction ID in the original request.
        /// </summary>
        [JsonProperty(PropertyName = "correlation_id")]
        [Required]
        public string CorrelationId { get; set; } = default!;

        /// <summary>
        /// Gets or set the name of the bucket where the files in are stored.
        /// </summary>
        [JsonProperty(PropertyName = "bucket")]
        [Required]
        public string Bucket { get; set; } = default!;

        /// <summary>
        /// For DIMSE, the sender or calling AE Title of the DICOM dataset.
        /// For an ACR inference request, the transaction ID.
        /// </summary>
        [JsonProperty(PropertyName = "calling_aetitle")]
        [Required]
        public string CallingAeTitle { get; set; } = default!;

        /// <summary>
        /// For DIMSE, the MONAI Deploy AE Title received the DICOM dataset.
        /// For an ACR inference request, this field is empty.
        /// </summary>
        [JsonProperty(PropertyName = "called_aetitle")]
        public string CalledAeTitle { get; set; } = default!;

        /// <summary>
        /// Gets or sets the time the data was received.
        /// </summary>
        [JsonProperty(PropertyName = "timestamp")]
        [Required]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets a list of files and metadata files in this request.
        /// </summary>
        [JsonProperty(PropertyName = "payload")]
        [Required, MinLength(1, ErrorMessage = "At least one file is required.")]
        public IReadOnlyList<BlockStorageInfo> Payload { get => _payload; }

        public WorkflowRequestEvent()
        {
            _payload = new List<BlockStorageInfo>();
            Workflows = new List<string>();
        }

        public void AddFiles(IEnumerable<BlockStorageInfo> files)
        {
            _payload.AddRange(files);
        }
    }
}
