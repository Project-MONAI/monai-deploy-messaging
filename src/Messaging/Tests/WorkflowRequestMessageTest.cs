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
    public class WorkflowRequestMessageTest
    {
        [Fact(DisplayName = "Converts JSONMessage to Message")]
        public void ConvertsJsonMessageToMessage()
        {
            var input = new WorkflowRequestEvent()
            {
                Bucket = Guid.NewGuid().ToString(),
                DataTrigger = new DataOrigin
                {
                    DataService = DataService.DIMSE,
                    Source = Guid.NewGuid().ToString(),
                    Destination = Guid.NewGuid().ToString(),
                },
                CorrelationId = Guid.NewGuid().ToString(),
                FileCount = 10,
                PayloadId = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                Workflows = new List<string> { Guid.NewGuid().ToString() },
                WorkflowInstanceId = Guid.NewGuid().ToString(),
                TaskId = Guid.NewGuid().ToString(),
            };
            input.DataOrigins.Add(new DataOrigin
            {
                DataService = DataService.DicomWeb,
                Source = Guid.NewGuid().ToString(),
                Destination = Guid.NewGuid().ToString(),

            });
            input.DataOrigins.Add(new DataOrigin
            {
                DataService = DataService.FHIR,
                Source = Guid.NewGuid().ToString(),
                Destination = Guid.NewGuid().ToString(),
            });
            input.DataOrigins.Add(new DataOrigin
            {
                DataService = DataService.DIMSE,
                Source = Guid.NewGuid().ToString(),
                Destination = Guid.NewGuid().ToString(),
            });
            input.DataOrigins.Add(new DataOrigin
            {
                DataService = DataService.HL7,
                Source = Guid.NewGuid().ToString(),
                Destination = Guid.NewGuid().ToString(),
            });

            var files = new List<BlockStorageInfo>()
                {
                    new BlockStorageInfo{ Path =Guid.NewGuid().ToString(), Metadata=Guid.NewGuid().ToString() },
                    new BlockStorageInfo{ Path =Guid.NewGuid().ToString(), Metadata=Guid.NewGuid().ToString() },
            };

            input.AddFiles(files);

            Assert.Equal(files, input.Payload);
        }
    }
}
