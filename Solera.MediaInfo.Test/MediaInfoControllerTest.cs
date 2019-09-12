using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Polly;
using Polly.Registry;
using Solera.MediaInfo.Service.Controllers;
using Solera.MediaInfo.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Xunit;

namespace Solera.MediaInfo.Service.Test
{
    [Trait("Category", "Unit")]
    public class MediaInfoControllerTest
    {
        #region Private members
        private MediaInfoController _sut;
        private readonly Mock<ILogger<MediaInfoController>> _mocklogger;
        private readonly Mock<IAmazonS3> _mockS3Client;
        private readonly Fixture _fixture;
        #endregion

        public MediaInfoControllerTest()
        {
            _fixture = new Fixture();
            Environment.SetEnvironmentVariable("S3_BUCKET", _fixture.Create("S3_BUCKET"));
            Mock<IReadOnlyPolicyRegistry<string>> mockPolicyRegistry = new Mock<IReadOnlyPolicyRegistry<string>>();
           mockPolicyRegistry.Setup(pol => pol.Get<IAsyncPolicy>("mbePolicy")).Returns(Policy.NoOpAsync());            
            _mocklogger = new Mock<ILogger<MediaInfoController>>();
            _mockS3Client = new Mock<IAmazonS3>();
            _sut = new MediaInfoController(mockPolicyRegistry.Object, _mockS3Client.Object, _mocklogger.Object);
        }

        #region PostPhotoEndpoint
        /// <summary>
        /// Post File to S3 on valid file name returns OK
        /// </summary>
        [Fact]
        public async void PostFileToSoleraS3_OnFileName_Returns_ExpectSuccess()
        {
            // Arrange
            var mockConfig = new Mock<IClientConfig>();
            mockConfig.SetupGet(_ => _.ServiceURL)
                .Returns(_fixture.Create("https://url"))
                .Verifiable();
            _mockS3Client.SetupGet(_ => _.Config)
                 .Returns(mockConfig.Object)
                 .Verifiable();
            IFormFile formfile = GetPhotoIFormFile("TestPhoto01.jpg", "A jpg file");

            // Act
            var response = await _sut.PostFileToSoleraS3(new UploadFileRequest() { TargetPath = "Some file", File = formfile }) as ObjectResult;

            // Assert
            Assert.NotNull(response);
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            response.Should().Equals("https://s3.gp2.axadmin.net/rms-development/Some file/TestPhoto01.jpg");
        }
        #endregion


        #region DeletePhotoEndpoint
        [Fact]
        public async void DeletePhotoEndpoint_Returns_SuccessResponse()
        {
            // Arrange
            var deleteFileRequest = _fixture.Create<DeleteFileRequest>();
            var s3ClientResponse = new DeleteObjectsResponse()
            {
                HttpStatusCode = HttpStatusCode.OK,
                DeleteErrors = new List<DeleteError>(),
                DeletedObjects = new List<DeletedObject>()
            };
            s3ClientResponse.DeletedObjects.AddRange(
                deleteFileRequest.Body.TargetPaths.Select(
                    t => new DeletedObject() { Key = t, DeleteMarker = true }).ToArray()
            );
            _mockS3Client.Setup(m => m.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(s3ClientResponse)
                .Verifiable();

            // Act
            var deleteFilesResponse = await _sut.DeleteFileFromSoleraS3(deleteFileRequest);

            // Assert
            _mockS3Client.Verify();
            Assert.NotNull(deleteFilesResponse);
            Assert.IsType<ObjectResult>(deleteFilesResponse);
            var objResult = (ObjectResult)deleteFilesResponse;
            Assert.Equal(StatusCodes.Status200OK, objResult.StatusCode);
            var response = (Response<string>)objResult.Value;
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.True(response.IsSuccess);
            Assert.Equal($"Successfully deleted {s3ClientResponse.DeletedObjects.Count} item(s)", response.Data);
        }

        [Fact]
        public async void DeletePhotoEndpoint_Returns_500_When_S3ClientThrowsException()
        {
            // Arrange
            var deleteFileRequest = _fixture.Create<DeleteFileRequest>();
            var s3ClientResponse = new DeleteObjectsResponse()
            {
                HttpStatusCode = HttpStatusCode.InternalServerError,
                DeleteErrors = new List<DeleteError>(),
                DeletedObjects = new List<DeletedObject>()
            };
            s3ClientResponse.DeleteErrors.AddRange(
                deleteFileRequest.Body.TargetPaths.Select(
                    t => new DeleteError() { Key = t,  Code = _fixture.Create("Code"), Message = _fixture.Create("Message")}).ToArray()
            );
            _mockS3Client.Setup(m => m.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DeleteObjectsException(s3ClientResponse))
                .Verifiable();

            // Act
            var deleteFilesResponse = await _sut.DeleteFileFromSoleraS3(deleteFileRequest);

            // Assert
            _mockS3Client.Verify();
            Assert.NotNull(deleteFilesResponse);
            Assert.IsType<ObjectResult>(deleteFilesResponse);
            var objResult = (ObjectResult)deleteFilesResponse;
            Assert.Equal(StatusCodes.Status500InternalServerError, objResult.StatusCode);
            var response = (Response<string>)objResult.Value;
            Assert.Equal(StatusCodes.Status500InternalServerError, response.StatusCode);
            Assert.False(response.IsSuccess);
            Assert.Contains($"No. of objects failed to delete = { s3ClientResponse.DeleteErrors.Count}", response.Message);
            Assert.Collection(response.Errors,
                e => Assert.Contains(s3ClientResponse.DeleteErrors[0].Key, e.Message),
                e => Assert.Contains(s3ClientResponse.DeleteErrors[1].Key, e.Message),
                e => Assert.Contains(s3ClientResponse.DeleteErrors[2].Key, e.Message));
        }
        #endregion

        #region Data Builders
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
        #endregion
    }
}
