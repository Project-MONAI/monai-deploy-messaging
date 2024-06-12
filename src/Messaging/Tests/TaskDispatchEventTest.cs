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
using System.Collections.Generic;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Xunit;

namespace Monai.Deploy.Messaging.Tests
{
    public class TaskDispatchEventTest
    {
        [Fact(DisplayName = "Validation throws on error")]
        public void ValidationThrowsOnError()
        {
            var taskDispatchEvent = new TaskDispatchEvent();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.WorkflowInstanceId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.ExecutionId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.PayloadId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.TaskId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.CorrelationId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.TaskPluginType = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.Inputs = new List<Storage>();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            var input = new Storage();
            taskDispatchEvent.Inputs.Add(input);
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.Outputs = new List<Storage>();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            var output = new Storage();
            taskDispatchEvent.Outputs.Add(output);
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            output.Name = "name";
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            output.Endpoint = "endpoint";
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            // Skip settings credentials for output, this shall not throw given that is not required

            output.Bucket = "bucket";
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            output.RelativeRootPath = "path";
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            var intermediate = new Storage();
            taskDispatchEvent.IntermediateStorage = intermediate;
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            intermediate.Name = "name";
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            intermediate.Endpoint = "endpoint";
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            intermediate.Bucket = "bucket";
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            intermediate.RelativeRootPath = "path";
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            input.Name = "name";
            input.Endpoint = "endpoint";
            input.Bucket = "bucket";
            input.RelativeRootPath = "path";

            var exception = Record.Exception(() => taskDispatchEvent.Validate());
            Assert.Null(exception);

            // Let's set the credentials for input, this should throw validation exception given that it's no longer null
            input.Credentials = new Credentials();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            input.Credentials.AccessKey = "key";
            input.Credentials.AccessToken = "token";
            exception = Record.Exception(() => taskDispatchEvent.Validate());
            Assert.Null(exception);
        }
    }
}
