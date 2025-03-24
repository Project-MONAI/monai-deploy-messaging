/*
 * Copyright 2021-2025 MONAI Consortium
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

using System.Collections;
using System.ComponentModel.DataAnnotations;
using Ardalis.GuardClauses;
using Monai.Deploy.Messaging.Common;

namespace Monai.Deploy.Messaging.Events
{
    public abstract class EventBase
    {
        /// <summary>
        /// Validates the message with all properties recursively.
        /// Throws <see cref="Monai.Deploy.Messaging.Common.MessageValidationException"/> on error.
        /// </summary>
        public virtual void Validate()
        {
            var validationContextItems = new Dictionary<object, object?>();
            var validationResults = new List<ValidationResult>();
            if (!TryValidateRecursively(this, validationContextItems, validationResults, new HashSet<object>(), GetType().Name))
            {
                throw new MessageValidationException(validationResults);
            }
        }

        private bool TryValidateRecursively<T>(T instance,
                                              IDictionary<object, object?> validationContextItems,
                                              List<ValidationResult> validationResults,
                                              ISet<object> validatedObjects,
                                              string propertyPath)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(validationContextItems, nameof(validationContextItems));
            Guard.Against.Null(validationResults, nameof(validationResults));
            Guard.Against.Null(validatedObjects, nameof(validatedObjects));
            Guard.Against.NullOrWhiteSpace(propertyPath, nameof(propertyPath));

            if (instance.GetType().IsGenericType)
            {
                return true;
            }

            if (validatedObjects.Contains(instance))
            {
                return true;
            }
            validatedObjects.Add(instance);

            var result = Validator.TryValidateObject(instance, new ValidationContext(instance, null, validationContextItems), validationResults, true);

            var properties = instance.GetType().GetProperties().Where(prop => prop.CanRead && prop.GetIndexParameters().Length == 0).ToList();

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string) || property.PropertyType.IsValueType) continue;

                var value = instance.GetType().GetProperty(property.Name)?.GetValue(instance, null);

                if (value == null)
                {
                    continue;
                }

                result &= TryValidateProperty(validationContextItems, validationResults, validatedObjects, value, $"{propertyPath}.{property.Name}");
            }

            return result;
        }

        private bool TryValidateProperty(IDictionary<object, object?> validationContextItems,
                                         List<ValidationResult> validationResults,
                                         ISet<object> validatedObjects,
                                         object? value,
                                         string propertyPath)
        {
            Guard.Against.Null(validationContextItems, nameof(validationContextItems));
            Guard.Against.Null(validationResults, nameof(validationResults));
            Guard.Against.Null(validatedObjects, nameof(validatedObjects));
            Guard.Against.NullOrWhiteSpace(propertyPath, nameof(propertyPath));

            var result = true;

            if (value is IEnumerable enumerable &&
                !TryValidateEnumerableRecursively(enumerable, validationContextItems, validationResults, validatedObjects, propertyPath))
            {
                result = false;
            }

            var nestedValidationResults = new List<ValidationResult>();
            if (!TryValidateRecursively(value, validationContextItems, nestedValidationResults, validatedObjects, propertyPath))
            {
                result = false;
                foreach (var validationResult in nestedValidationResults)
                {
                    validationResults.Add(new ValidationResult(validationResult.ErrorMessage, validationResult.MemberNames.Select(p => propertyPath + '.' + p)));
                }
            }

            return result;
        }

        private bool TryValidateEnumerableRecursively(IEnumerable enumerable,
                                                      IDictionary<object, object?> validationContextItems,
                                                      IList<ValidationResult> validationResults,
                                                      ISet<object> validatedObjects,
                                                      string propertyPath)
        {
            Guard.Against.Null(enumerable, nameof(enumerable));
            Guard.Against.Null(validationContextItems, nameof(validationContextItems));
            Guard.Against.Null(validationResults, nameof(validationResults));
            Guard.Against.Null(validatedObjects, nameof(validatedObjects));
            Guard.Against.NullOrWhiteSpace(propertyPath, nameof(propertyPath));

            var result = true;
            foreach (var enumObj in enumerable)
            {
                if (enumObj is not null)
                {
                    var nestedValidationResults = new List<ValidationResult>();
                    if (!TryValidateRecursively(enumObj, validationContextItems, nestedValidationResults, validatedObjects, propertyPath))
                    {
                        result = false;
                        foreach (var validationResult in nestedValidationResults)
                        {
                            validationResults.Add(new ValidationResult(validationResult.ErrorMessage, validationResult.MemberNames.Select(p => $"{propertyPath}.{p}")));
                        }
                    }
                }
            }
            return result;
        }
    }
}
