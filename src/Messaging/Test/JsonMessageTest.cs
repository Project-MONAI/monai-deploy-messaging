// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System;
using Monai.Deploy.Messaging.Messages;
using Xunit;

namespace Monai.Deploy.Messaging.Test
{
    public class JsonMessageTest
    {
        [Fact(DisplayName = "Converts JSONMessage to Message")]
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
    }
}
