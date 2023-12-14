/*
 * Copyright 2022-2023 MONAI Consortium
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
using Xunit;

namespace Monai.Deploy.Messaging.Tests.Common
{
    public class ArtifactTypesTests
    {
        [Fact]
        public void ArtifactTypeValid_ShouldReturnTrue()
        {
            Assert.True(ArtifactTypes.Validate("CR"));
        }

        [Fact]
        public void ArtifactTypeInvalid_ShouldReturnFalse()
        {
            Assert.False(ArtifactTypes.Validate("false"));
        }

        [Fact]
        public void ArtifactTypeNull_ShouldReturnFalse()
        {
            Assert.False(ArtifactTypes.Validate(null));
        }

        [Fact]
        public void ArtifactTypes_Should_Contain_All()
        {
            foreach (var artifactType in Enum.GetValues(typeof(ArtifactType)))
            {
                Assert.True(ArtifactTypes.ListOfModularity.ContainsKey((ArtifactType)artifactType));
            }
        }

        [Fact]
        public void ArtifactTypes_Should_Contain_Same_Count()
        {
            Assert.Equal(ArtifactTypes.ListOfModularity.Count, Enum.GetValues(typeof(ArtifactType)).Length);
        }

    }
}
