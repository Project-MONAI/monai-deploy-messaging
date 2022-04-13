// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Monai.Deploy.Messaging.Events
{
    public class RunnerCompleteEvent : EventBase
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
        /// Gets or sets the correlation ID.
        /// </summary>
        [JsonProperty(PropertyName = "correlation_id")]
        [Required]
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the name of the job.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        [Required, MaxLength(63)]
        public string Name { get; set; }

        public RunnerCompleteEvent()
        {
            WorkflowId = String.Empty;
            TaskId = String.Empty;
            CorrelationId = String.Empty;
            Name = String.Empty;
        }
    }
}
