/*
 * Copyright 2021-2024 MONAI Consortium
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
using Newtonsoft.Json.Converters;

namespace Monai.Deploy.Messaging.Events
{
    public class TaskCancellationEvent : EventBase
    {
        /// <summary>
        /// Gets or sets the ID representing the instance of the workflow.
        /// </summary>
        [JsonProperty(PropertyName = "workflow_instance_id")]
        [JsonPropertyName("workflow_instance_id")]
        [Required]
        public string WorkflowInstanceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the execution ID representing the instance of the task.
        /// </summary>
        [JsonProperty(PropertyName = "execution_id")]
        [JsonPropertyName("execution_id")]
        [Required]
        public string ExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID representing the instance of the Task.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "task_id")]
        [JsonPropertyName("task_id")]
        public string TaskId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identity provided by the external service.
        /// </summary>
        [JsonProperty(PropertyName = "identity")]
        [JsonPropertyName("identity")]
        [Required, MaxLength(63)]
        public string Identity { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the reason for the cancellation.
        /// </summary>
        [JsonProperty(PropertyName = "reason")]
        [JsonPropertyName("reason")]
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public FailureReason Reason { get; set; } = FailureReason.Unknown;

        /// <summary>
        /// Gets or sets a message relating to the cancellation.
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        [JsonPropertyName("message")]
        [Required, MaxLength(63)]
        public string Message { get; set; } = string.Empty;
    }
}
