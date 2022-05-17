// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0
//
using System.Runtime.Serialization;

namespace Monai.Deploy.Messaging.Common
{
    [Serializable]
    public class MessageConversionException : Exception
    {
        public MessageConversionException()
        {
        }

        public MessageConversionException(string message) : base(message)
        {
        }

        public MessageConversionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MessageConversionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
