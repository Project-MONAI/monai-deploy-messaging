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
    }
}
