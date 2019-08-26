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
using Solera.MediaInfo.Service.Helpers;
using Polly;
using System.Threading.Tasks;
using Polly.Timeout;

namespace Solera.MediaInfo.Service.Test
{
    [Trait("Category", "Unit")]
    public class MediaInfoControllerTest
    {
        #region Private members
        private MediaInfoController _sut;
        private Mock<HttpContext> _mockContext;
        private Mock<HttpRequest> _mockRequest;
        
        #endregion

        public MediaInfoControllerTest()
        {
            _mockRequest = new Mock<HttpRequest>();
            _mockContext = new Mock<HttpContext>();
            Environment.SetEnvironmentVariable("S3_ACCESS_KEY", "BNDW1H4EWBFDJ36H61F3");
            Environment.SetEnvironmentVariable("S3_SECRET_KEY", "WClsE8kfoP8g29yCyF6BtZeukbnEwMzVWPVDO03g");
            Environment.SetEnvironmentVariable("S3_URL", "https://s3.gp2.axadmin.net");
            Environment.SetEnvironmentVariable("S3_BUCKET", "rms-development");
            _mockContext.Setup(c => c.Request).Returns(_mockRequest.Object);
            Mock<IReadOnlyPolicyRegistry<string>> mockPolicyRegistry = new Mock<IReadOnlyPolicyRegistry<string>>();
           mockPolicyRegistry.Setup(pol => pol.Get<IAsyncPolicy>("mbePolicy")).Returns(Policy.NoOpAsync());
           _sut = new MediaInfoController(mockPolicyRegistry.Object);
            
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
        /// Post File to S3 on valid file name returns OK
        /// </summary>
        [Fact]
        public async void PostFileToSoleraS3_OnFileName_Returns_ExpectSuccess()
        {
           
            IFormFile formfile = GetPhotoIFormFile("TestPhoto01.jpg", "A jpg file");
            var response = await _sut.PostFileToSoleraS3("Some file", formfile) as ObjectResult;
            Assert.NotNull(response);
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            response.Should().Equals("https://s3.gp2.axadmin.net/rms-development/Some file/TestPhoto01.jpg");
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
