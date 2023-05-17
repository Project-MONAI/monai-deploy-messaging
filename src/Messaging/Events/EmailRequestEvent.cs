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
using Newtonsoft.Json;

namespace Monai.Deploy.Messaging.Events
{
    public class EmailRequestEvent : EventBase
    {
        [JsonProperty(PropertyName = "id")]
        [Required]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "workflow_instance_id")]
        [Required]
        public string WorkflowInstanceId { get; set; }

        [JsonProperty(PropertyName = "workflow_name")]
        [Required]
        public string WorkflowName { get; set; }

        [JsonProperty(PropertyName = "task_id")]
        [Required]
        public string TaskId { get; set; }

        [JsonProperty(PropertyName = "roles")]
        public string Roles { get; set; }

        [JsonProperty(PropertyName = "emails")]
        public string Emails { get; set; }

        [Required]
        [JsonProperty(PropertyName = "metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        public EmailRequestEvent()
        {
            Id = Guid.NewGuid();
            WorkflowInstanceId = string.Empty;
            TaskId = string.Empty;
            WorkflowName = string.Empty;
            Metadata = new Dictionary<string, string>();
            Roles = string.Empty;
            Emails = string.Empty;
        }
    }
}
