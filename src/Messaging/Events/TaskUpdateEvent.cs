﻿// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Monai.Deploy.Messaging.Events
{
    public class TaskUpdateEvent : EventBase
    {
        /// <summary>
        /// Gets or sets the ID representing the instance of the workflow.
        /// </summary>
        [JsonProperty(PropertyName = "workflow_id")]
        [Required]
        public string WorkflowId { get; set; }

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
        public TaskStatus Status { get; set; }

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
        /// Gets or sets any metadata relevant to the output of the task.
        /// </summary>
        [JsonProperty(PropertyName = "metadata")]
        public Dictionary<string, object> Metadata { get; set; }

        public TaskUpdateEvent()
        {
            WorkflowId = String.Empty;
            TaskId = String.Empty;
            ExecutionId = String.Empty;
            CorrelationId = String.Empty;
            Status = TaskStatus.Unknown;
            Reason = FailureReason.None;
            Message = String.Empty;
        }
    }
}
