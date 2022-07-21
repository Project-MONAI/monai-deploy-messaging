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
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Messages;
using Xunit;

namespace Monai.Deploy.Messaging.Tests
{
    public class DummyTypeOne
    {
        public string? MyProperty { get; set; }
    }

    public class DummyTypeTwo
    {
        public int MyProperty { get; set; }
    }

    public class JsonMessageTest
    {
        [Fact(DisplayName = "Convert throws on different type")]
        public void ConvertsThrowsError()
        {
            var data = new DummyTypeOne { MyProperty = "hello world" };
            var jsonMessage = new JsonMessage<DummyTypeOne>(data, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var message = jsonMessage.ToMessage();

            Assert.Throws<MessageConversionException>(() => message.ConvertTo<DummyTypeTwo>());
            Assert.Throws<MessageConversionException>(() => message.ConvertToJsonMessage<DummyTypeTwo>());
        }

        [Fact(DisplayName = "Converts JsonMessage to Message")]
        public void ConvertsJsonMessageToMessage()
        {
            var expected = "hello world";
            var jsonMessage = new JsonMessage<string>(expected, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var message = jsonMessage.ToMessage();

            Assert.Equal(jsonMessage.ApplicationId, message.ApplicationId);
            Assert.Equal(jsonMessage.CreationDateTime, message.CreationDateTime);
            Assert.Equal(jsonMessage.ContentType, message.ContentType);
            Assert.Equal(jsonMessage.CorrelationId, message.CorrelationId);
            Assert.Equal(jsonMessage.DeliveryTag, message.DeliveryTag);
            Assert.Equal(jsonMessage.MessageDescription, message.MessageDescription);
            Assert.Equal(jsonMessage.MessageId, message.MessageId);

            var result = message.ConvertTo<string>();

            Assert.Equal(expected, result);
        }

        [Fact(DisplayName = "Converts Message to JsonMessage")]
        public void ConvertsMessageToJsonMessage()
        {
            var expected = "hello world";
            var jsonMessage = new JsonMessage<string>(expected, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var message = jsonMessage.ToMessage();

            Assert.Equal(jsonMessage.ApplicationId, message.ApplicationId);
            Assert.Equal(jsonMessage.CreationDateTime, message.CreationDateTime);
            Assert.Equal(jsonMessage.ContentType, message.ContentType);
            Assert.Equal(jsonMessage.CorrelationId, message.CorrelationId);
            Assert.Equal(jsonMessage.DeliveryTag, message.DeliveryTag);
            Assert.Equal(jsonMessage.MessageDescription, message.MessageDescription);
            Assert.Equal(jsonMessage.MessageId, message.MessageId);

            var result = message.ConvertToJsonMessage<string>();

            Assert.Equal(expected, result.Body);
            Assert.Equal(jsonMessage.ApplicationId, result.ApplicationId);
            Assert.Equal(jsonMessage.CreationDateTime, result.CreationDateTime);
            Assert.Equal(jsonMessage.ContentType, result.ContentType);
            Assert.Equal(jsonMessage.CorrelationId, result.CorrelationId);
            Assert.Equal(jsonMessage.DeliveryTag, result.DeliveryTag);
            Assert.Equal(jsonMessage.MessageDescription, result.MessageDescription);
            Assert.Equal(jsonMessage.MessageId, result.MessageId);
        }
    }
}
