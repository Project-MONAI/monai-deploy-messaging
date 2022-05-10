// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using System.Collections.Generic;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Xunit;

namespace Monai.Deploy.Messaging.Test
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
