// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Messages;
using Xunit;

namespace Monai.Deploy.Messaging.Test
{
    public class WorkflowRequestMessageTest
    {
        [Fact(DisplayName = "Converts JSONMessage to Message")]
        public void ConvertsJsonMessageToMessage()
        {
            var input = new WorkflowRequestMessage()
            {
                Bucket = Guid.NewGuid().ToString(),
                CalledAeTitle = Guid.NewGuid().ToString(),
                CallingAeTitle = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                FileCount = 10,
                PayloadId = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                Workflows = new List<string> { Guid.NewGuid().ToString() }
            };

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
