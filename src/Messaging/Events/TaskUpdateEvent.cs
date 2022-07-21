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

using System.ComponentModel.DataAnnotations;
using Monai.Deploy.Messaging.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Monai.Deploy.Messaging.Events
{
    public class TaskUpdateEvent : EventBase
    {
        /// <summary>
        /// Gets or sets the ID representing the instance of the workflow.
        /// </summary>
        [JsonProperty(PropertyName = "workflow_instance_id")]
        [Required]
        public string WorkflowInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the ID representing the instance of the Task.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "task_id")]
        public string TaskId { get; set; }

        /// <summary>
        /// Gets or sets the execution ID representing the instance of the task.
        /// </summary>
        [JsonProperty(PropertyName = "execution_id")]
        [Required]
        public string ExecutionId { get; set; }

        /// <summary>
        /// Gets or sets the correlation ID.
        /// </summary>
        [JsonProperty(PropertyName = "correlation_id")]
        [Required]
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or set the status of the task.
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        [JsonConverter(typeof(StringEnumConverter))]
        [Required]
        public TaskExecutionStatus Status { get; set; }

        /// <summary>
        /// Gets or set the failure reason of the task.
        /// </summary>
        [JsonProperty(PropertyName = "reason")]
        [JsonConverter(typeof(StringEnumConverter))]
        [Required]
        public FailureReason Reason { get; set; }

        /// <summary>
        /// Gets or set any additional (error) message related to the task.
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets any output artifacts relevent to the output of the task.
        /// </summary>
        [JsonProperty(PropertyName = "outputs")]
        public List<Storage> Outputs { get; set; }

        /// <summary>
        /// Gets or sets any metadata relevant to the output of the task.
        /// </summary>
        [JsonProperty(PropertyName = "metadata")]
        public Dictionary<string, object> Metadata { get; set; }

        public TaskUpdateEvent()
        {
            WorkflowInstanceId = String.Empty;
            TaskId = String.Empty;
            ExecutionId = String.Empty;
            CorrelationId = String.Empty;
            Status = TaskExecutionStatus.Unknown;
            Reason = FailureReason.None;
            Message = String.Empty;
            Metadata = new Dictionary<string, object>();
            Outputs = new List<Storage>();
        }
    }
}
