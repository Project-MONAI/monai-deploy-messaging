// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.ComponentModel.DataAnnotations;
using Monai.Deploy.Messaging.Common;
using Newtonsoft.Json;

namespace Monai.Deploy.Messaging.Events
{
    public class TaskDispatchEvent : EventBase
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
        /// Gets or sets the fully qualified assembly name of the task plug-in for the task.
        /// </summary>
        [JsonProperty(PropertyName = "task_assembly_name")]
        [Required]
        public string? TaskAssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the task execution arguments.
        /// </summary>
        [JsonProperty(PropertyName = "arguments")]
        public Dictionary<string, object>? Arguments { get; set; }

        /// <summary>
        /// Gets or sets the input storage information.
        /// </summary>
        [JsonProperty(PropertyName = "inputs")]
        [Required, MinLength(1, ErrorMessage = "At least input is required.")]
        public List<Storage> Inputs { get; set; }

        /// <summary>
        /// Gets or sets the output storage information.
        /// </summary>
        [JsonProperty(PropertyName = "outputs")]
        [Required]
        public List<Storage> Outputs { get; set; }

        public TaskDispatchEvent()
        {
            Inputs = new List<Storage>();
            Outputs = new List<Storage>();
        }
    }
}
