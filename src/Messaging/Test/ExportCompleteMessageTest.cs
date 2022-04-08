// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using Monai.Deploy.Messaging.Events;
using Xunit;

namespace Monai.Deploy.Messaging.Test
{
    public class ExportCompleteMessageTest
    {
        [Theory(DisplayName = "Shall generate ExportCompleteMessageTest from ExportRequestMessage")]
        [InlineData(1, 0, ExportStatus.Success)]
        [InlineData(0, 5, ExportStatus.Failure)]
        [InlineData(3, 3, ExportStatus.PartialFailure)]
        public void ShallGenerateExportCompleteMessageTestFromExportRequestMessage(int successded, int fialure, ExportStatus status)
        {
            var exportRequestMessage = new ExportRequestEvent
            {
                CorrelationId = Guid.NewGuid().ToString(),
                DeliveryTag = Guid.NewGuid().ToString(),
                Destination = Guid.NewGuid().ToString(),
                ExportTaskId = Guid.NewGuid().ToString(),
                MessageId = Guid.NewGuid().ToString(),
                WorkflowId = Guid.NewGuid().ToString(),
                SucceededFiles = successded,
                FailedFiles = fialure,
            };
            exportRequestMessage.Files = new List<string>()
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
            };

            var errors = new List<string>()
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
            };

            exportRequestMessage.AddErrorMessages(errors);

            var exportCompleteMessage = new ExportCompleteEvent(exportRequestMessage);

            Assert.Equal(exportRequestMessage.WorkflowId, exportCompleteMessage.WorkflowId);
            Assert.Equal(exportRequestMessage.ExportTaskId, exportCompleteMessage.ExportTaskId);
            Assert.Equal(string.Join(System.Environment.NewLine, errors), exportCompleteMessage.Message);
            Assert.Equal(status, exportCompleteMessage.Status);
        }
    }
}
