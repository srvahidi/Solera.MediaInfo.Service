using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Moq;
using Solera.MediaInfo.Service.Middleware;
using System;
using Xunit;

namespace Solera.MediaInfo.Test.Extensions
{
    [Trait("Category", "Unit")]
    public class HealthCheckMiddlewareExtensionsTests
    {
        #region Private members
        private readonly Fixture _fixture;
        #endregion

        public HealthCheckMiddlewareExtensionsTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());
        }

        [Fact]
        public void UseHealthCheckMiddleware_Should_CallUseMiddleware()
        {
            // Arrange
            var mockApp = new Mock<IApplicationBuilder>();
            mockApp.Setup(_ => _.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
                .Verifiable();

            // Act
            HealthCheckMiddlewareExtensions.UseHealthCheckMiddleware(mockApp.Object);

            // Assert
            mockApp.Verify();
        }
    }
}
