/*
 * Copyright 2021-2024 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
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
    public class TaskUpdateEventTest
    {
        [Fact(DisplayName = "Validation throws on error")]
        public void ValidationThrowsOnError()
        {
            var taskDispatchEvent = new TaskUpdateEvent();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.WorkflowInstanceId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.ExecutionId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.TaskId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.CorrelationId = Guid.NewGuid().ToString();
            var exception = Record.Exception(() => taskDispatchEvent.Validate());
            Assert.Null(exception);
        }
    }
}
