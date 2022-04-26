// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.ComponentModel.DataAnnotations;
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
        /// Gets or sets the fully qualified assembly name of the task plug-in for the task.
        /// </summary>
        [JsonProperty(PropertyName = "task_assembly_name")]
        [Required]
        public string TaskAssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the task execution arguments.
        /// </summary>
        [JsonProperty(PropertyName = "task_plugin_arguments")]
        public Dictionary<string, string> TaskPluginArguments { get; set; }

        /// <summary>
        /// Gets or set the status of the task.
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        [JsonConverter(typeof(StringEnumConverter))]
        [Required]
        public TaskStatus Status { get; set; }

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

        /// <summary>
        /// Gets or sets any metadata relevant to the task.
        /// </summary>
        [JsonProperty(PropertyName = "metadata")]
        public Dictionary<string, object> Metadata { get; set; }

        public TaskDispatchEvent()
        {
            WorkflowId = String.Empty;
            TaskId = String.Empty;
            ExecutionId = String.Empty;
            CorrelationId = String.Empty;
            TaskAssemblyName = String.Empty;
            TaskPluginArguments = new Dictionary<string, string>();
            Status = TaskStatus.Unknown;
            Inputs = new List<Storage>();
            Outputs = new List<Storage>();
            Metadata = new Dictionary<string, object>();
        }
    }
}
