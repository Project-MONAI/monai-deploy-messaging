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

using System.ComponentModel.DataAnnotations;

namespace Monai.Deploy.Messaging.Common
{
    public class MessageValidationException : Exception
    {
        public MessageValidationException(List<ValidationResult> errors)
            : base(FormatMessage(errors))
        {
        }

        private static string FormatMessage(List<ValidationResult> errors)
        {
            if (errors is null || errors.Count == 0)
            {
                return "Invalid message.";
            }

            return $"Invalid message: {string.Join(',', errors.Select(p => $"{p.ErrorMessage} Path: {string.Join(',', p.MemberNames)}."))}";
        }
    }
}
