// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Monai.Deploy.Messaging.Common
{
    [Serializable]
    public class MessageValidationException : Exception
    {
        public MessageValidationException()
        {
        }

        public MessageValidationException(List<ValidationResult> errors)
            : base(FormatMessage(errors))
        {
        }

        protected MessageValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        private static string FormatMessage(List<ValidationResult> errors)
        {
            return $"Invalid message: {string.Join(',', errors.Select(p => p.ErrorMessage))}";
        }
    }
}
