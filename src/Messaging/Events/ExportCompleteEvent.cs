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
using Ardalis.GuardClauses;
using Monai.Deploy.Messaging.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Monai.Deploy.Messaging.Events
{
    public class ExportCompleteEvent : EventBase
    {
        /// <summary>
        /// Gets or sets the workflow instanceID generated by the Workflow Manager.
        /// </summary>
        [JsonProperty(PropertyName = "workflow_instance_id")]
        [Required]
        public string WorkflowInstanceId { get; set; } = default!;

        /// <summary>
        /// Gets or sets the export task ID generated by the Workflow Manager.
        /// </summary>
        [JsonProperty(PropertyName = "export_task_id")]
        [Required]
        public string ExportTaskId { get; set; } = default!;

        /// <summary>
        /// Gets or sets the state of the export task.
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        [JsonConverter(typeof(StringEnumConverter))]
        [Required]
        public ExportStatus Status { get; set; }

        /// <summary>
        /// Gets or sets error messages, if any, when exporting.
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; } = default!;

        /// <summary>
        /// Gets or sets files exported with its status
        /// </summary>
        [JsonProperty(PropertyName = "file_statuses")]
        public Dictionary<string, FileExportStatus> FileStatuses { get; set; }

        /// <summary>
        /// Gets or set the ExportRequest type.
        /// For standard exports this will be ExportRequestType.None
        /// but for exports to external apps this will be ExportRequestType.ExternalProcessing
        /// </summary>
        [JsonProperty(PropertyName = "export_request")]
        [Required]
        public ExportRequestType ExportRequest { get; set; } = default!;

        [JsonConstructor]
        public ExportCompleteEvent()
        {
            Status = ExportStatus.Unknown;
            FileStatuses = new Dictionary<string, FileExportStatus>();
        }

        public ExportCompleteEvent(ExportRequestEvent exportRequest, ExportStatus exportStatus, Dictionary<string, FileExportStatus> fileStatuses)
        {
            Guard.Against.Null(exportRequest, nameof(exportRequest));
            Guard.Against.Null(exportStatus, nameof(exportStatus));
            Guard.Against.Null(fileStatuses, nameof(fileStatuses));

            WorkflowInstanceId = exportRequest.WorkflowInstanceId;
            ExportTaskId = exportRequest.ExportTaskId;
            Message = string.Join(System.Environment.NewLine, exportRequest.ErrorMessages);
            Status = exportStatus;
            FileStatuses = fileStatuses;
        }
    }
}
