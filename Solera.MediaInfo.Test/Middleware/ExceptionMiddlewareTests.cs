using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Polly;
using Polly.Registry;
using Solera.MediaInfo.Service.Controllers;
using Solera.MediaInfo.Service.Middleware;
using Solera.MediaInfo.Service.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Xunit;

namespace Solera.MediaInfo.Service.Test
{
    [Trait("Category", "Unit")]
    public class ExceptionMiddlewareTest : IDisposable
    {
        #region Private members
        private ExceptionMiddleware _sut;
        private Mock<ILogger<ExceptionMiddleware>> _mockLogger;
        private Mock<RequestDelegate> _mockRequestDelegate;
        #endregion

        public ExceptionMiddlewareTest()
        {
            _mockRequestDelegate = new Mock<RequestDelegate>();            
            _mockLogger = new Mock<ILogger<ExceptionMiddleware>>();
            _sut = new ExceptionMiddleware(_mockRequestDelegate.Object, _mockLogger.Object);
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
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            var streamText = reader.ReadToEnd();
            var objResponse = JsonConvert.DeserializeObject<Response<string>>(streamText);

            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
            Assert.Equal(StatusCodes.Status500InternalServerError, objResponse.StatusCode);
            Assert.False(objResponse.IsSuccess);
            Assert.NotEmpty(objResponse.Message);
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
