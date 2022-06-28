// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Xunit;

namespace Monai.Deploy.Messaging.Tests
{
    public class ExportCompleteEventTest
    {
        [Fact(DisplayName = "Shall generate ExportCompleteMessageTest from ExportRequestMessage")]
        public void ShallGenerateExportCompleteMessageTestFromExportRequestMessage()
        {
            var exportRequestMessage = new ExportRequestEvent
            {
                CorrelationId = Guid.NewGuid().ToString(),
                DeliveryTag = Guid.NewGuid().ToString(),
                Destinations = new string[] { Guid.NewGuid().ToString() },
                ExportTaskId = Guid.NewGuid().ToString(),
                MessageId = Guid.NewGuid().ToString(),
                WorkflowInstanceId = Guid.NewGuid().ToString(),
                Files = new List<string>()
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                }
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

            var exportCompleteMessage = new ExportCompleteEvent(exportRequestMessage, ExportStatus.Success);

            Assert.Equal(exportRequestMessage.WorkflowInstanceId, exportCompleteMessage.WorkflowInstanceId);
            Assert.Equal(exportRequestMessage.ExportTaskId, exportCompleteMessage.ExportTaskId);
            Assert.Equal(string.Join(Environment.NewLine, errors), exportCompleteMessage.Message);
            Assert.Equal(ExportStatus.Success, exportCompleteMessage.Status);
        }

        [Fact(DisplayName = "Validation shall throw on error")]
        public void ValidationShallThrowOnError()
        {
            var exportCompleteEvent = new ExportCompleteEvent();

            Assert.Throws<MessageValidationException>(() => exportCompleteEvent.Validate());
        }
    }
}
