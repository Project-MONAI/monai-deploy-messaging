﻿/*
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
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Xunit;

namespace Monai.Deploy.Messaging.Tests
{
    public class TaskCancellationEventTests
    {
        [Fact(DisplayName = "Validation throws on error")]
        public void ValidationThrowsOnError()
        {
            var runnerComplete = new TaskCancellationEvent();
            Assert.Throws<MessageValidationException>(() => runnerComplete.Validate());

            runnerComplete.WorkflowInstanceId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => runnerComplete.Validate());

            runnerComplete.TaskId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => runnerComplete.Validate());

            runnerComplete.ExecutionId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => runnerComplete.Validate());

            runnerComplete.Identity = "1234567890123456789012345678901234567890123456789012345678901234567890";
            Assert.Throws<MessageValidationException>(() => runnerComplete.Validate());

            runnerComplete.Reason = FailureReason.Unknown;
            Assert.Throws<MessageValidationException>(() => runnerComplete.Validate());

            runnerComplete.Message = "1234567890123456789012345678901234567890123456789012345678901234567890";
            Assert.Throws<MessageValidationException>(() => runnerComplete.Validate());

            runnerComplete.Message = "123456789012345678901234567890123456789012345678901234567890123";
            runnerComplete.Identity = "123456789012345678901234567890123456789012345678901234567890123";
            var exception = Record.Exception(() => runnerComplete.Validate());
            Assert.Null(exception);
        }
    }
}
