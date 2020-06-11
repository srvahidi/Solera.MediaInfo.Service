using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Solera.MediaInfo.Service.Middleware;
using Solera.MediaInfo.Service.Models;
using System;
using System.IO;
using Xunit;

namespace Solera.MediaInfo.Test.Unit.Middleware
{
    [Trait("Category", "Unit")]
    public class HealthCheckMiddlewareTests : IDisposable
    {
        #region Private members
        private readonly Fixture _fixture;
        private HealthCheckMiddleware _sut;
        private Mock<ILogger<HealthCheckMiddleware>> _mockLogger;
        #endregion

        public HealthCheckMiddlewareTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());

            _mockLogger = _fixture.Freeze<Mock<ILogger<HealthCheckMiddleware>>>();
            _sut = _fixture.Create<HealthCheckMiddleware>();
        }

        [Fact]
        public async void InvokeAsync_Should_CallNextAndComplete()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            _mockLogger.Setup(m => m.Log(
               LogLevel.Information,
               It.IsAny<EventId>(),
               "Media Info Service is Ok",
               It.IsAny<Exception>(),
               It.IsAny<Func<object, Exception, string>>()));

            // Act
            await _sut.InvokeAsync(context);

            // Assert
            _mockLogger.Verify();

            Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            var streamText = reader.ReadToEnd();
            var objResponse = JsonConvert.DeserializeObject<Response<string>>(streamText);
            Assert.Equal(StatusCodes.Status200OK, objResponse.StatusCode);
            Assert.True(objResponse.IsSuccess);
            Assert.NotEmpty(objResponse.Data);

            var jObj = JObject.Parse(streamText);
            Assert.True(jObj.ContainsKey("isSuccess"), "should return camel case json");
        }

        public void Dispose()
        {
            if (_sut != null)
            {
                var disposable = _sut as IDisposable;
                disposable?.Dispose();
            }
        }
    }
}
