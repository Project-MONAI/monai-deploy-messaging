// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Xunit;

namespace Monai.Deploy.Messaging.Test
{
    public class RunnerCompleteEventTest
    {
        [Fact(DisplayName = "Validation throws on error")]
        public void ValidationThrowsOnError()
        {
            var runnerComplete = new TaskCallbackEvent();
            Assert.Throws<MessageValidationException>(() => runnerComplete.Validate());

            runnerComplete.WorkflowInstanceId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => runnerComplete.Validate());

            runnerComplete.TaskId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => runnerComplete.Validate());

            runnerComplete.ExecutionId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => runnerComplete.Validate());

            runnerComplete.CorrelationId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => runnerComplete.Validate());

            runnerComplete.Identity = "1234567890123456789012345678901234567890123456789012345678901234567890";
            Assert.Throws<MessageValidationException>(() => runnerComplete.Validate());

            runnerComplete.Identity = "123456789012345678901234567890123456789012345678901234567890123";
            var exception = Record.Exception(() => runnerComplete.Validate());
            Assert.Null(exception);
        }
    }
}
