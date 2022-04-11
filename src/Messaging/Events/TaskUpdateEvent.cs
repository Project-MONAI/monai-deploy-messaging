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
        public string? WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the ID representing the instance of the Task.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "task_id")]
        public string? TaskId { get; set; }

        /// <summary>
        /// Gets or sets the correlation ID.
        /// </summary>
        [JsonProperty(PropertyName = "correlation_id")]
        [Required]
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Gets or set the status of the task.
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        [JsonConverter(typeof(StringEnumConverter))]
        [Required]
        public TaskStatus Status { get; set; } = TaskStatus.NotRun;

        /// <summary>
        /// Gets or set the failure reason of the task.
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        [JsonConverter(typeof(StringEnumConverter))]
        [Required]
        public FailureReason Reason { get; set; } = FailureReason.None;

        /// <summary>
        /// Gets or set any additional (error) message related to the task.
        /// </summary>
        [JsonProperty(PropertyName = "reason")]
        public string? Message { get; set; }
    }
}
