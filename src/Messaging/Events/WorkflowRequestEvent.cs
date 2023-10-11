/*
 * Copyright 2021-2023 MONAI Consortium
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
using System.Text.Json.Serialization;
using Monai.Deploy.Messaging.Common;
using Newtonsoft.Json;

namespace Monai.Deploy.Messaging.Events
{
    public class DataOrigin
    {
        [JsonProperty(PropertyName = "dataService")]
        [JsonPropertyName("dataService")]
        public DataService DataService { get; set; } = DataService.Unknown;

        /// <summary>
        /// Gets or sets the source of the data:
        /// <list type="bullet">
        /// <item>DIMSE: the sender or calling AE Title of the DICOM dataset.</item>
        /// <item>ACR inference request: the transaction ID.</item>
        /// <item>FHIR/HL7: host name or IP address.</item>
        /// <item>DICOMWeb: host name or IP address.</item>
        /// </list>
        /// </summary>
        [JsonProperty(PropertyName = "source")]
        [JsonPropertyName("source")]
        [Required]
        public string Source { get; set; } = default!;

        /// <summary>
        /// Gets or set the receiving service.
        /// <list type="bullet">
        /// <item>DIMSE: the MONAI Deploy AE Title that received the DICOM dataset.</item>
        /// <item>ACR inference request: IP address of the receiving service.</item>
        /// <item>FHIR/HL7: IP address of the receiving service.</item>
        /// <item>DICOMWeb: IP address of the receiving service or the named virtual AE Title.</item>
        /// </list>
        /// </summary>
        [JsonProperty(PropertyName = "destination")]
        [JsonPropertyName("destination")]
        public string Destination { get; set; } = default!;

        [JsonProperty(PropertyName = "fromExternalApp")]
        [JsonPropertyName("fromExternalApp")]
        public bool FromExternalApp { get; set; } = false;

        public override int GetHashCode()
        {
            return HashCode.Combine(Source, Destination, DataService);
        }

        public override bool Equals(object? obj)
        {
            return obj is DataOrigin dataOrigin &&
                Source.Equals(dataOrigin.Source, StringComparison.Ordinal) &&
                Destination.Equals(dataOrigin.Destination, StringComparison.Ordinal) &&
                DataService.Equals(dataOrigin.DataService);
        }
    }

    public enum DataService
    {
        /// <summary>
        /// Unknown data service
        /// </summary>
        Unknown,

        /// <summary>
        /// Data received via DIMSE services
        /// </summary>
        DIMSE,

        /// <summary>
        /// Data received via DICOMWeb services
        /// </summary>
        DicomWeb,

        /// <summary>
        /// Data received via FHIR services
        /// </summary>
        FHIR,

        /// <summary>
        /// Data received via HL7 services
        /// </summary>
        HL7,

        /// <summary>
        /// Data received via ACR API call
        /// </summary>
        ACR,
    }

    public class WorkflowRequestEvent : EventBase
    {
        private readonly List<BlockStorageInfo> _payload;

        /// <summary>
        /// Gets or sets the ID of the payload which is also used as the root path of the payload.
        /// </summary>
        [JsonProperty(PropertyName = "payload_id")]
        [JsonPropertyName("payload_id")]
        [Required]
        public Guid PayloadId { get; set; }

        /// <summary>
        /// Gets or sets the associated workflows to be launched.
        /// </summary>
        [JsonProperty(PropertyName = "workflows")]
        [JsonPropertyName("workflows")]
        public IEnumerable<string> Workflows { get; set; }

        /// <summary>
        /// Gets or sets number of files in the payload.
        /// </summary>
        [JsonProperty(PropertyName = "file_count")]
        [JsonPropertyName("file_count")]
        [Required]
        public int FileCount { get; set; }

        /// <summary>
        /// For DIMSE, the correlation ID is the UUID associated with the first DICOM association received.
        /// For an ACR inference request, the correlation ID is the Transaction ID in the original request.
        /// </summary>
        [JsonProperty(PropertyName = "correlation_id")]
        [JsonPropertyName("correlation_id")]
        [Required]
        public string CorrelationId { get; set; } = default!;

        /// <summary>
        /// Gets or set the name of the bucket where the files in are stored.
        /// </summary>
        [JsonProperty(PropertyName = "bucket")]
        [JsonPropertyName("bucket")]
        [Required]
        public string Bucket { get; set; } = default!;

        /// <summary>
        /// Gets or sets the service that received the original request.
        /// </summary>
        [JsonProperty(PropertyName = "trigger")]
        [JsonPropertyName("trigger")]
        public DataOrigin DataTrigger { get; set; } = default!;

        /// <summary>
        /// Gets or sets the data origins that were involved in triggering this workflow request.
        /// </summary>
        [JsonProperty(PropertyName = "data_origins")]
        [JsonPropertyName("data_origins")]
        public List<DataOrigin> DataOrigins { get; private set; }

        /// <summary>
        /// Gets or sets the time the data was received.
        /// </summary>
        [JsonProperty(PropertyName = "timestamp")]
        [JsonPropertyName("timestamp")]
        [Required]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Sets the workflow instance ID for the Workflow Manager.
        /// This is only applicable to resume events (after external app executions)
        /// In standard workflows this will not be set
        /// </summary>
        [JsonProperty(PropertyName = "workflow_instance_id")]
        [JsonPropertyName("workflow_instance_id")]
        public string? WorkflowInstanceId { get; set; } = default;

        /// <summary>
        /// Sets the task ID for the Workflow Manager.
        /// This is only applicable to resume events (after external app executions)
        /// In standard workflows this will not be set
        /// </summary>
        [JsonProperty(PropertyName = "task_id")]
        [JsonPropertyName("task_id")]
        public string? TaskId { get; set; } = default;

        /// <summary>
        /// Gets or sets a list of files and metadata files in this request.
        /// </summary>
        [JsonProperty(PropertyName = "payload")]
        [Required, MinLength(1, ErrorMessage = "At least one file is required.")]
        [JsonPropertyName("payload")]
        public IReadOnlyList<BlockStorageInfo> Payload { get => _payload; }

        public WorkflowRequestEvent()
        {
            _payload = new List<BlockStorageInfo>();
            Workflows = new List<string>();
            DataOrigins = new List<DataOrigin>();
        }

        public void AddFiles(IEnumerable<BlockStorageInfo> files)
        {
            _payload.AddRange(files);
        }
    }
}
