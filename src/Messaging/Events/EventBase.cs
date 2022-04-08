// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.ComponentModel.DataAnnotations;
using Monai.Deploy.Messaging.Common;

namespace Monai.Deploy.Messaging.Events
{
    public class EventBase
    {
        /// <summary>
        /// Validates the message.
        /// Throws <see cref="Monai.Deploy.Messaging.Common.MessageValidationException"/> on error.
        /// </summary>
        public void Validate()
        {
            var validationContext = new ValidationContext(this, null, null);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(this, validationContext, validationResults))
            {
                throw new MessageValidationException(validationResults);
            }
        }
    }
}
