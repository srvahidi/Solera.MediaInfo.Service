using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Moq;
using Solera.MediaInfo.Service.Middleware;
using System;
using Xunit;

namespace Solera.MediaInfo.Test.Extensions
{
    [Trait("Category", "Unit")]
    public class ExceptionMiddlewareExtensionsTests
    {
        #region Private members
        private readonly Fixture _fixture;
        #endregion

        public ExceptionMiddlewareExtensionsTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());
        }

        [Theory, AutoData]
        public void ConfigureCustomExceptionMiddleware_Should_CallUseMiddleware(string product, string layer)
        {
            // Arrange
            var mockApp = new Mock<IApplicationBuilder>();
            mockApp.Setup(_ => _.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
                .Verifiable();

            // Act
            ExceptionMiddlewareExtensions.ConfigureCustomExceptionMiddleware(mockApp.Object, product, layer);

            // Assert
            mockApp.Verify();
        }
    }
}
