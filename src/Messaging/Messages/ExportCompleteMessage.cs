﻿// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Ardalis.GuardClauses;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Monai.Deploy.Messaging.Messages
{
    public enum ExportStatus
    {
        Success = 0,
        Failure,
        PartialFailure,
        Unknown
    }

    public class ExportCompleteMessage
    {
        /// <summary>
        /// Gets or sets the workflow ID generated by the Workflow Manager.
        /// </summary>
        public string WorkflowId { get; set; } = default!;

        /// <summary>
        /// Gets or sets the export task ID generated by the Workflow Manager.
        /// </summary>
        public string ExportTaskId { get; set; } = default!;

        /// <summary>
        /// Gets or sets the state of the export task.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ExportStatus Status { get; set; }

        /// <summary>
        /// Gets or sets error messages, if any, when exporting.
        /// </summary>
        public string Message { get; set; } = default!;

        [JsonConstructor]
        public ExportCompleteMessage()
        {
            Status = ExportStatus.Unknown;
        }

        public ExportCompleteMessage(ExportRequestMessage exportRequest)
        {
            Guard.Against.Null(exportRequest, nameof(exportRequest));

            WorkflowId = exportRequest.WorkflowId;
            ExportTaskId = exportRequest.ExportTaskId;
            Message = string.Join(System.Environment.NewLine, exportRequest.ErrorMessages);

            if (exportRequest.FailedFiles == 0)
            {
                Status = ExportStatus.Success;
            }
            else if (exportRequest.FailedFiles == exportRequest.Files.Count())
            {
                Status = ExportStatus.Failure;
            }
            else
            {
                Status = ExportStatus.PartialFailure;
            }
        }
    }
}
