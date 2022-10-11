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

            var fileStatus = new Dictionary<string, FileExportStatus>()
            {
                {Guid.NewGuid().ToString(), FileExportStatus.Success},
                {Guid.NewGuid().ToString(), FileExportStatus.DownloadError},
                {Guid.NewGuid().ToString(), FileExportStatus.ConfigurationError},
            };

            exportRequestMessage.AddErrorMessages(errors);

            var exportCompleteMessage = new ExportCompleteEvent(exportRequestMessage, ExportStatus.Success, fileStatus);

            Assert.Equal(exportRequestMessage.WorkflowInstanceId, exportCompleteMessage.WorkflowInstanceId);
            Assert.Equal(exportRequestMessage.ExportTaskId, exportCompleteMessage.ExportTaskId);
            Assert.Equal(string.Join(Environment.NewLine, errors), exportCompleteMessage.Message);
            Assert.Equal(ExportStatus.Success, exportCompleteMessage.Status);
            Assert.Equal(fileStatus, exportCompleteMessage.FileStatuses);
        }

        [Fact(DisplayName = "Validation shall throw on error")]
        public void ValidationShallThrowOnError()
        {
            var exportCompleteEvent = new ExportCompleteEvent();

            Assert.Throws<MessageValidationException>(() => exportCompleteEvent.Validate());
        }
    }
}
