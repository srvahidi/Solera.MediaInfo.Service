using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Polly;
using Polly.Registry;
using Solera.MediaInfo.Service.Constants;
using Solera.MediaInfo.Service.Controllers;
using Solera.MediaInfo.Service.Helpers;
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
        private readonly Mock<ITransferUtility> _mockTransferUtility;
        private readonly Fixture _fixture;
        private const string HOST = "http://testhost.net";
        private const string BUCKET = "testbucket";
        #endregion

        public MediaInfoControllerTest()
        {
            _fixture = new Fixture();
            Environment.SetEnvironmentVariable(EnvironmentVariable.S3_BUCKET, BUCKET);
            Mock<IReadOnlyPolicyRegistry<string>> mockPolicyRegistry = new Mock<IReadOnlyPolicyRegistry<string>>();
            mockPolicyRegistry.Setup(pol => pol.Get<IAsyncPolicy>("mbePolicy")).Returns(Policy.NoOpAsync());
            _mocklogger = new Mock<ILogger<MediaInfoController>>();
            var mockConfig = new Mock<IClientConfig>();
            mockConfig.SetupGet(_ => _.ServiceURL)
                .Returns(HOST)
                .Verifiable();
            _mockS3Client = new Mock<IAmazonS3>();
            _mockS3Client.SetupGet(_ => _.Config)
                 .Returns(mockConfig.Object)
                 .Verifiable();
            _mockTransferUtility = new Mock<ITransferUtility>();
            var mockTransferUtilitySimpleFactory = new Mock<ITransferUtilitySimpleFactory>();
            mockTransferUtilitySimpleFactory.Setup(_ => _.Create(It.IsAny<IAmazonS3>()))
                .Returns(_mockTransferUtility.Object);
            _sut = new MediaInfoController(mockPolicyRegistry.Object, _mockS3Client.Object, mockTransferUtilitySimpleFactory.Object, _mocklogger.Object);
        }

        #region PostPhotoEndpoint
        [Fact]
        public async void PostFileToSoleraS3_Returns_Success()
        {
            // Arrange
            var targetPath = _fixture.Create("target path");
            var fileName = _fixture.Create("file name");
            IFormFile formfile = GetPhotoIFormFile(fileName, _fixture.Create("file content"));

            // Act
            var postFileResponse = await _sut.PostFileToSoleraS3(new UploadFileRequest() { TargetPath = targetPath, File = formfile }) as ObjectResult;

            // Assert
            Assert.NotNull(postFileResponse);
            Assert.IsType<ObjectResult>(postFileResponse);
            Assert.Equal(StatusCodes.Status200OK, postFileResponse.StatusCode);
            var response = (Response<string>)postFileResponse.Value;
            Assert.True(response.IsSuccess);
            Assert.Equal($"{HOST}/{BUCKET}/{targetPath}/{fileName}", response.Data);
        }

        [Theory]
        [MemberData(nameof(GetExceptions))]
        public async void PostFileToSoleraS3_Should_ThrowException_When_ErrorFromTransferUtility(Exception exception)
        {
            // Arrange
            var targetPath = _fixture.Create("target path");
            var fileName = _fixture.Create("file name");
            IFormFile formfile = GetPhotoIFormFile(fileName, _fixture.Create("file content"));

            _mockTransferUtility.Setup(_ => _.UploadAsync(
                It.IsAny<TransferUtilityUploadRequest>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            var result = await Assert.ThrowsAsync(exception.GetType(), async () =>
                await _sut.PostFileToSoleraS3(new UploadFileRequest() { TargetPath = targetPath, File = formfile }));

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Message);
        }
        #endregion


        #region DeletePhotoEndpoint
        [Fact]
        public async void DeletePhotoEndpoint_Returns_SuccessResponse()
        {
            // Arrange
            var deleteFileRequest = new DeleteFileRequest()
            {
                Body = new RemoveFileBody()
                {
                    TargetPaths = _fixture.CreateMany($"{HOST}/{BUCKET}/", 3).ToArray()
                }
            };
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
            var deleteFileRequest = new DeleteFileRequest()
            {
                Body = new RemoveFileBody()
                {
                    TargetPaths = _fixture.CreateMany($"{HOST}/{BUCKET}/", 3).ToArray()
                }
            };
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

        public static IEnumerable<object[]> GetExceptions()
        {
            var allData = new List<object[]>
            {
                new object[] { new AmazonS3Exception(new Exception())},
                new object[] { new WebException() },
            };
            return allData;
        }
        #endregion
    }
}
