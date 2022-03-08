using Avalanche.Api.Helpers;
using Avalanche.Shared.Infrastructure.Configuration;
using Moq;
using Xunit;

namespace Avalanche.Api.Test.Helpers
{
    public class ConfigurationHelperTest
    {
        [Fact]
        public void SetupConfigurationInformationTest()
        {
            // Arrange
            var setupHelper = SetupConfigurationHelper.PatientInfoHelper();
            var isValidate = false;

            // Act
            foreach (var p in setupHelper)
            {
                var keyUpper = $"{p.Key}".ToUpper();
                var valueUpper = $"{p.Value.Name}".ToUpper();

                isValidate = keyUpper == valueUpper;

                if (!isValidate)
                {
                    break;
                }
            }

            // Assert
            Assert.True(isValidate);
        }
    }
}
