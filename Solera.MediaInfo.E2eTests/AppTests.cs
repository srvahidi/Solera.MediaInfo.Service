using Amazon.S3.Model;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Solera.MediaInfo.E2eTests.Utilities;
using Solera.MediaInfo.Service.Models;
using System;
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

            appFixture.S3ApiMock.Reset();
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

        // TODO: Run localStack or something similar in a docket container to test the S3 service
        //[Fact]
        //public async Task Mis_WhenMediaInfoUrlInvoked_ShouldReturnAnItem()
        //{
        //    // Arrange
        //    _appFixture.S3ApiMock.Setup(s => s.HandlePost(
        //        It.Is<Uri>(u => u.PathAndQuery == "/"),
        //        It.IsAny<HttpContext>()))
        //        .Callback<Uri, HttpContext>(async (uri, context) => {
        //            await context.Response.WriteAsync("1");
        //        });

        //    MultipartFormDataContent requestContent = new MultipartFormDataContent();
        //    requestContent.Add(new ByteArrayContent(FixtureHelper.GetFixtureBytes("Fixtures/optimus.jpg")), "file", "optimus.jpg");
        //    requestContent.Add(new StringContent("autosource/valuation-number/optimus.jpg"), "targetpath");

        //    // Act
        //    var response = await _appFixture.Client.PostAsync($"{_appFixture.MisUrl}/api/mediainfo/file", requestContent);

        //    // Assert
        //    response.EnsureSuccessStatusCode();
        //    var jActual = (JObject)await response.Content.ReadAsJSONAsync();
        //    jActual["isSuccess"].Value<bool>().Should().Be(true);
        //    jActual["data"].Value<string>().Should().NotBeNullOrWhiteSpace();
        //}


        [Theory, AutoData]
        public async Task Mis_WhenMediaInfoUrlInvoked_ShouldReturnAnItem(
            DeleteFileRequest deleteFileRequest)
        {
            // Arrange
            deleteFileRequest.Body.TargetPaths = deleteFileRequest.Body.TargetPaths.Take(1).ToArray();
            var s3Response = new DeleteObjectsResponse()
            {
                DeletedObjects = new System.Collections.Generic.List<DeletedObject>()
                {
                    new DeletedObject(){Key = deleteFileRequest.Body.TargetPaths.First() }
                },
                
            };
            _appFixture.S3ApiMock.Setup(s => s.HandlePost(
                It.Is<Uri>(u => u.PathAndQuery == "/"),
                It.IsAny<HttpContext>()))
                .Callback<Uri, HttpContext>(async (uri, context) => {
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(s3Response));
                });

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
