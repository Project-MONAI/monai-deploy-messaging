/*
 * Copyright 2022-2023 MONAI Consortium
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
using Newtonsoft.Json.Converters;

namespace Monai.Deploy.Messaging.Events
{
    public class TaskDispatchEvent : EventBase
    {
        /// <summary>
        /// Gets or sets the ID representing the instance of the workflow.
        /// </summary>
        [JsonProperty(PropertyName = "workflow_instance_id")]
        [JsonPropertyName("workflow_instance_id")]
        [Required]
        public string WorkflowInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the ID representing the instance of the Task.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "task_id")]
        [JsonPropertyName("task_id")]
        public string TaskId { get; set; }

        /// <summary>
        /// Gets or sets the execution ID representing the instance of the task.
        /// </summary>
        [JsonProperty(PropertyName = "execution_id")]
        [JsonPropertyName("execution_id")]
        [Required]
        public string ExecutionId { get; set; }

        /// <summary>
        /// Gets or sets the payload ID of the current workflow instance.
        /// </summary>
        [JsonProperty(PropertyName = "payload_id")]
        [JsonPropertyName("payload_id")]
        [Required]
        public string PayloadId { get; set; }

        /// <summary>
        /// Gets or sets the correlation ID.
        /// </summary>
        [JsonProperty(PropertyName = "correlation_id")]
        [JsonPropertyName("correlation_id")]
        [Required]
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the type of plug-in the task is associated with.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        [JsonPropertyName("type")]
        [Required]
        public string TaskPluginType { get; set; }

        /// <summary>
        /// Gets or sets the task execution arguments.
        /// </summary>
        [JsonProperty(PropertyName = "task_plugin_arguments")]
        [JsonPropertyName("task_plugin_arguments")]
        public Dictionary<string, string> TaskPluginArguments { get; set; }

        /// <summary>
        /// Gets or set the status of the task.
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        [JsonPropertyName("status")]
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public TaskExecutionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the input storage information.
        /// </summary>
        [JsonProperty(PropertyName = "inputs")]
        [JsonPropertyName("inputs")]
        [Required, MinLength(1, ErrorMessage = "At least input is required.")]
        public List<Storage> Inputs { get; set; }

        /// <summary>
        /// Gets or sets the output storage information.
        /// </summary>
        [JsonProperty(PropertyName = "outputs")]
        [JsonPropertyName("outputs")]
        [Required]
        public List<Storage> Outputs { get; set; }

        /// <summary>
        /// Gets or sets the intermediate storage information.
        /// </summary>
        [JsonProperty(PropertyName = "intermediate_storage")]
        [JsonPropertyName("intermediate_storage")]
        [Required]
        public Storage IntermediateStorage { get; set; }

        /// <summary>
        /// Gets or sets any metadata relevant to the task.
        /// </summary>
        [JsonProperty(PropertyName = "metadata")]
        [JsonPropertyName("metadata")]
        public Dictionary<string, object> Metadata { get; set; }

        public TaskDispatchEvent()
        {
            WorkflowInstanceId = string.Empty;
            TaskId = string.Empty;
            ExecutionId = string.Empty;
            CorrelationId = string.Empty;
            PayloadId = string.Empty;
            TaskPluginType = string.Empty;
            TaskPluginArguments = new Dictionary<string, string>();
            Inputs = new List<Storage>();
            Outputs = new List<Storage>();
            IntermediateStorage = null!;
            Metadata = new Dictionary<string, object>();
        }
    }
}
