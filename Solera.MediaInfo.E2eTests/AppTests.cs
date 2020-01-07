using AutoFixture.Xunit2;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Solera.MediaInfo.E2eTests.Utilities;
using Solera.MediaInfo.Service.Models;
using System.Linq;
using System.Net.Http;
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

            appFixture.Client.DefaultRequestHeaders.Clear();
        }

        [Fact]
        public async Task Mis_WhenBaseUrlInvoked_ShouldReturnHealthCheck()
        {
            // Arrange

            // Act
            var response = await _appFixture.Client.GetAsync($"{_appFixture.MisUrl}/api/health");

            // Assert
            response.EnsureSuccessStatusCode();
            var jActual = (JObject)await response.Content.ReadAsJsonAsync();
            jActual["isSuccess"].Value<bool>().Should().Be(true);
        }

        // TODO: Run localStack or something similar in a docker container to test the S3 service
        //[Fact]
        //public async Task Mis_WhenMediaInfoUrlInvoked_ShouldUploadAnItem()
        //{
        //    // Arrange
        //    MultipartFormDataContent requestContent = new MultipartFormDataContent();
        //    requestContent.Add(new ByteArrayContent(FixtureHelper.GetFixtureBytes("Fixtures/optimus.jpg")), "file", "optimus.jpg");
        //    requestContent.Add(new StringContent("autosource/valuation-number/optimus.jpg"), "targetpath");

        //    // Act
        //    var response = await _appFixture.Client.PostAsync($"{_appFixture.MisUrl}/api/mediainfo/file", requestContent);

        //    // Assert
        //    response.EnsureSuccessStatusCode();
        //    var jActual = (JObject)await response.Content.ReadAsJsonAsync();
        //    jActual["isSuccess"].Value<bool>().Should().Be(true);
        //    jActual["data"].Value<string>().Should().NotBeNullOrWhiteSpace();
        //}


        [Theory, AutoData]
        public async Task Mis_WhenMediaInfoUrlInvoked_ShouldReturnSuccess(
            DeleteFileRequest deleteFileRequest)
        {
            // Arrange
            deleteFileRequest.Body.TargetPaths = deleteFileRequest.Body.TargetPaths.Take(1).ToArray();

            var message = new HttpRequestMessage(HttpMethod.Delete, $"{_appFixture.MisUrl}/api/mediainfo");
            message.Content = deleteFileRequest.Body.ToJsonStringContent();

            // Act
            var response = await _appFixture.Client.SendAsync(message);

            // Assert
            response.EnsureSuccessStatusCode();
            var jActual = (JObject)await response.Content.ReadAsJsonAsync();
            jActual["isSuccess"].Value<bool>().Should().Be(true);
            jActual["data"].Value<string>().Should().NotBeNullOrWhiteSpace();
        }
    }
}
