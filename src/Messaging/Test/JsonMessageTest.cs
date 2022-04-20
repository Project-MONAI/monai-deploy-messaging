// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using Monai.Deploy.Messaging.Messages;
using Xunit;

namespace Monai.Deploy.Messaging.Test
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
        [Fact(DisplayName = "Convert returns null on different type")]
        public void ConvertsReturnsNull()
        {
            var data = new DummyTypeOne { MyProperty = "hello world" };
            var jsonMessage = new JsonMessage<DummyTypeOne>(data, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var message = jsonMessage.ToMessage();


            var result = message.ConvertTo<DummyTypeTwo>();
            Assert.Null(result);

            var jsonMessageResult = message.ConvertToJsonMessage<DummyTypeTwo>();
            Assert.Null(jsonMessageResult);
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
