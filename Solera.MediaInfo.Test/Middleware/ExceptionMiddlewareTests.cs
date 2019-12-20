using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Solera.MediaInfo.Service.Middleware;
using Solera.MediaInfo.Service.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Solera.MediaInfo.Service.Test
{
    [Trait("Category", "Unit")]
    public class ExceptionMiddlewareTests : IDisposable
    {
        #region Private members
        private readonly Fixture _fixture;
        private readonly ExceptionMiddleware _sut;
        private readonly Mock<RequestDelegate> _mockRequestDelegate;
        #endregion

        public ExceptionMiddlewareTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());

            _mockRequestDelegate = _fixture.Freeze<Mock<RequestDelegate>>();
            _fixture.Inject(_mockRequestDelegate.Object);
            _sut = _fixture.Create<ExceptionMiddleware>();
        }

        [Fact]
        public async void InvokeAsync_Should_CallNextAndComplete()
        {
            // Arrange
            var context = new DefaultHttpContext();
            _mockRequestDelegate.Setup(next => next(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _sut.InvokeAsync(context);

            // Assert
            _mockRequestDelegate.Verify();
        }

        [Fact]
        public async void InvokeAsync_Returns_500_When_CatchException()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            _mockRequestDelegate.Setup(next => next(It.IsAny<HttpContext>()))
                .Throws<Exception>()
                .Verifiable();

            // Act
            await _sut.InvokeAsync(context);

            // Assert
            _mockRequestDelegate.Verify();
            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            var streamText = reader.ReadToEnd();
            var objResponse = JsonConvert.DeserializeObject<Response<string>>(streamText);
            Assert.Equal(StatusCodes.Status500InternalServerError, objResponse.StatusCode);
            Assert.False(objResponse.IsSuccess);
            Assert.NotEmpty(objResponse.Message);

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
