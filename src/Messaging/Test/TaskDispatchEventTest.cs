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

            taskDispatchEvent.WorkflowId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.ExecutionId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.TaskId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.CorrelationId = Guid.NewGuid().ToString();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            taskDispatchEvent.TaskAssemblyName = Guid.NewGuid().ToString();
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

            output.Credentials = new Credentials();
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            output.Credentials.AccessToken = "token";
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            output.Credentials.AccessKey = "key";
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            output.Bucket = "bucket";
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            output.RelativeRootPath = "path";
            Assert.Throws<MessageValidationException>(() => taskDispatchEvent.Validate());

            input.Name = "name";
            input.Endpoint = "endpoint";
            input.Credentials = new Credentials
            {
                AccessToken = "token",
                AccessKey = "key"
            };
            input.Bucket = "bucket";
            input.RelativeRootPath = "path";
            var exception = Record.Exception(() => taskDispatchEvent.Validate());
            Assert.Null(exception);
        }
    }
}
