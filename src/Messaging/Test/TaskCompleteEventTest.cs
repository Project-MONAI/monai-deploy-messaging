// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Xunit;

namespace Monai.Deploy.Messaging.Test
{
    public class TaskCompleteEventTest
    {
        [Fact(DisplayName = "Validation throws on error")]
        public void ValidationThrowsOnError()
        {
            var taskDispatchEvent = new TaskCompleteEvent();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.WorkflowId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.TaskId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.CorrelationId = Guid.NewGuid().ToString();
            var exception = Record.Exception(() => taskDispatchEvent.Validate());
            Assert.Null(exception);
        }
    }
}
