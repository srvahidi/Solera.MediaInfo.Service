using System;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Amazon.S3;
using Amazon.S3.Transfer;
using System.Threading;
using Amazon.S3.Model;
using System.Net;
using Amazon.Runtime;
using Amazon;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using Solera.MediaInfo.Service.Controllers;
using Polly.Registry;

namespace Solera.MediaInfo.Service.Test
{
    [Trait("Category", "Unit")]
    public class MediaInfoControllerTest
    {
        #region Private members
        private MediaInfoController _sut;
        private Mock<HttpContext> _mockContext;
        private Mock<HttpRequest> _mockRequest;
        private Mock<IAmazonS3> mockAmazonS3;
        #endregion

        public MediaInfoControllerTest()
        {
            _mockRequest = new Mock<HttpRequest>();
            _mockContext = new Mock<HttpContext>();

            _mockContext.Setup(c => c.Request).Returns(_mockRequest.Object);
            Mock<IReadOnlyPolicyRegistry<string>> polyRegistry = new Mock<IReadOnlyPolicyRegistry<string>>();
            _sut = new MediaInfoController(polyRegistry.Object);

        }
        /// <summary>
        /// Post File to S3 on empty file name throws an exception an InternalServerError http status code is expected.
        /// </summary>
        [Fact]
        public async void PostFileToSoleraS3_OnEmptyFileName_ExpectInternalServerErrorHttpStatusCode()
        {
            IFormFile formfile = GetPhotoIFormFile("TestPhoto01.jpg", "Hello World from a Fake File");
            var response = await _sut.PostFileToSoleraS3("", formfile) as ObjectResult;
            Assert.NotNull(response);
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.StatusCode);
        }
        /// <summary>
        /// Post File to S3 on empty file name throws an exception an InternalServerError http status code is expected.
        /// </summary>
        [Fact]
        public async void PostFileToSoleraS3_OnFileName_Returns_ExpectError()
        {
            mockAmazonS3 = new Mock<IAmazonS3>();
            mockAmazonS3.Setup(x => x.GetObjectAsync(
       It.IsAny<string>(),
       It.IsAny<string>(),
       It.IsAny<CancellationToken>()))
        .ReturnsAsync(
            (string bucket, string key, CancellationToken ct) =>
         new GetObjectResponse
         {
             BucketName = bucket,
             Key = key,
             HttpStatusCode = HttpStatusCode.OK

         });

            IFormFile formfile = GetPhotoIFormFile("TestPhoto01.jpg", "Hello World from a Fake File");
            var response = await _sut.PostFileToSoleraS3("Some file", formfile) as ObjectResult;
            Assert.NotNull(response);
            Assert.IsType<ObjectResult>(response);
            response.Should().Equals("Error: No RegionEndpoint or ServiceURL configured");
        }
        private static IFormFile GetPhotoIFormFile(string fileName, string fileContent)
        {
            //Setup mock file using a memory stream
            var ms = new System.IO.MemoryStream();
            var writer = new System.IO.StreamWriter(ms);
            writer.Write(fileContent);
            writer.Flush();
            ms.Position = 0;
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(ms.Length);
            mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
            return mockFile.Object;
        }
    }
}
