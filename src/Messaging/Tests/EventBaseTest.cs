// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monai.Deploy.Messaging.Common;
using Monai.Deploy.Messaging.Events;
using Xunit;

namespace Monai.Deploy.Messaging.Tests
{
    internal class StringClass : EventBase
    {
        [Required]
        public string? StringField { get; set; }
    }

    internal class NestedStringClass : EventBase
    {
        [Required]
        public StringClass? NestedStringField { get; set; }
    }

    internal class NestedStringCollectionClass : EventBase
    {
        [Required, MinLength(1)]
        public IList<StringClass>? NestedStrings { get; set; }
    }

    public class EventBaseTest
    {
        [Fact(DisplayName = "Validates StringClass")]
        public void ValidatesStringField()
        {
            var obj = new StringClass();
            var validationException = Assert.Throws<MessageValidationException>(() => obj.Validate());
            Assert.Equal($"Invalid message: The {nameof(obj.StringField)} field is required. Path: {nameof(obj.StringField)}.", validationException.Message);

            obj.StringField = "hello";
            var exception = Record.Exception(() => obj.Validate());
            Assert.Null(exception);
        }

        [Fact(DisplayName = "Validates NestedStringClass")]
        public void ValidatesNestedStringField()
        {
            var obj = new NestedStringClass();
            var validationException = Assert.Throws<MessageValidationException>(() => obj.Validate());
            Assert.Equal($"Invalid message: The {nameof(obj.NestedStringField)} field is required. Path: {nameof(obj.NestedStringField)}.", validationException.Message);

            obj.NestedStringField = new StringClass();
            validationException = Assert.Throws<MessageValidationException>(() => obj.Validate());
            Assert.Equal($"Invalid message: The {nameof(obj.NestedStringField.StringField)} field is required. Path: {nameof(NestedStringClass)}.{nameof(obj.NestedStringField)}.{nameof(obj.NestedStringField.StringField)}.", validationException.Message);

            obj.NestedStringField.StringField = "hello";
            var exception = Record.Exception(() => obj.Validate());
            Assert.Null(exception);
        }

        [Fact(DisplayName = "Validates NestedStringCollectionClass")]
        public void ValidatesNestedStringCollectionClassField()
        {
            var obj = new NestedStringCollectionClass();
            var validationException = Assert.Throws<MessageValidationException>(() => obj.Validate());
            Assert.Equal($"Invalid message: The {nameof(obj.NestedStrings)} field is required. Path: {nameof(obj.NestedStrings)}.", validationException.Message);

            obj.NestedStrings = new List<StringClass>();
            validationException = Assert.Throws<MessageValidationException>(() => obj.Validate());
            Assert.Equal($"Invalid message: The field {nameof(obj.NestedStrings)} must be a string or array type with a minimum length of '1'. Path: {nameof(obj.NestedStrings)}.", validationException.Message);

            var stringClass = new StringClass();
            obj.NestedStrings = new List<StringClass>() { stringClass };
            validationException = Assert.Throws<MessageValidationException>(() => obj.Validate());
            Assert.Equal($"Invalid message: The {nameof(stringClass.StringField)} field is required. Path: {nameof(NestedStringCollectionClass)}.{nameof(obj.NestedStrings)}.{nameof(StringClass.StringField)}.", validationException.Message);

            stringClass.StringField = "Hello World!";
            var exception = Record.Exception(() => obj.Validate());
            Assert.Null(exception);
        }
    }
}
