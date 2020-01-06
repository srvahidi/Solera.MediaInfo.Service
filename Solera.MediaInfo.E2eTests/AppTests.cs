using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Solera.MediaInfo.E2eTests
{
    [Trait("Category", "E2E")]
    public class AppTests : IClassFixture<AppFixture>
    {
        private readonly AppFixture _appFixture;

        public AppTests(AppFixture appFixture)
        {
            _appFixture = appFixture;

            appFixture.S3ApiMock.Reset();
            appFixture.Client.DefaultRequestHeaders.Clear();
        }

        [Fact]
        public async Task Mas_WhenBaseUrlInvoked_ShouldReturnHealthCheck()
        {
            // Arrange

            // Act
            var response = await _appFixture.Client.GetAsync($"{_appFixture.MisUrl}/api/health");

            // Assert
            response.EnsureSuccessStatusCode();
            var jActual = (JObject)await response.Content.ReadAsJSONAsync();
            jActual["isSuccess"].Value<bool>().Should().Be(true);
        }
    }
}
