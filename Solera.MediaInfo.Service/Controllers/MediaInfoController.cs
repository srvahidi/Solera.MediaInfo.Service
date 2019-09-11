using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Registry;
using Solera.MediaInfo.Service.Filters;
using Solera.MediaInfo.Service.Helpers;
using Solera.MediaInfo.Service.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Solera.MediaInfo.Service.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class MediaInfoController : ControllerBase
    {
        #region Private members
        private readonly string _s3AccessKey;
        private readonly string _s3SecretKey;
        private readonly string _s3url;
        private readonly string _s3bucket;
        private const string NullEmptyArgumentMessage = "Cannot be null or empty.";
        private readonly IReadOnlyPolicyRegistry<string> polyRegistry;
        private readonly ILogger _logger;
        #endregion

        public MediaInfoController(IReadOnlyPolicyRegistry<string> polyRegistry, ILogger<MediaInfoController> logger)
        {
            _logger = logger;
            _s3AccessKey = Environment.GetEnvironmentVariable("S3_ACCESS_KEY");
            _s3SecretKey = Environment.GetEnvironmentVariable("S3_SECRET_KEY");
            _s3url = Environment.GetEnvironmentVariable("S3_URL");
            _s3bucket = Environment.GetEnvironmentVariable("S3_BUCKET");
            this.polyRegistry = polyRegistry;
        }

        /// <summary>
        /// Upload file to Solera S3 service
        /// </summary>
        /// <param name="uploadFileRequest">File and full name in the target bucket</param>
        /// <returns></returns>
        // POST: api/MediaInfo
        [Consumes("multipart/form-data")]
        [HttpPost("file")]
        public async Task<object> PostFileToSoleraS3(UploadFileRequest uploadFileRequest)
        {
            AmazonS3Config config = new AmazonS3Config();
            config.ServiceURL = _s3url;
            IAmazonS3 _s3Client = new AmazonS3Client(_s3AccessKey, _s3SecretKey, config);

            StringBuilder bucketPath = new StringBuilder();
            bucketPath.Append(_s3bucket).Append("/").Append(uploadFileRequest.TargetPath).ToString();
            using (var memStream = new MemoryStream())
            {
                uploadFileRequest.File.CopyTo(memStream);
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = bucketPath.ToString(),
                    InputStream = memStream,
                    StorageClass = S3StorageClass.StandardInfrequentAccess,
                    PartSize = 6291456, // 6 MB.
                    Key = uploadFileRequest.File.FileName,
                    CannedACL = S3CannedACL.PublicRead
                };

                var fileTransferUtility = new TransferUtility(_s3Client);
                Logging.LogInformation("POST S4 : The UploadAsync is invoked with memsetream length: {0}, S3 Bucket: {1}, S3 target file: {2}",
                    memStream.Length, _s3bucket, uploadFileRequest.TargetPath);
                var policyAsync = polyRegistry.Get<IAsyncPolicy>("mbePolicy");
                await policyAsync.ExecuteAsync(async () =>
                {
                    await callFileTransfer(fileTransferUtility, fileTransferUtilityRequest);
                });

                string imageUrl = $"{ _s3url}/{_s3bucket}/{uploadFileRequest.TargetPath}/{uploadFileRequest.File.FileName}";
                Console.WriteLine($"Returned Url is {imageUrl}");
                return StatusCode(StatusCodes.Status200OK, new
                {
                    IsSuccess = true,
                    Data = imageUrl
                });
            }
        }

        private async Task callFileTransfer(TransferUtility transferutility, TransferUtilityUploadRequest filetransferuploadrequest)
        {
            try
            {
                await transferutility.UploadAsync(filetransferuploadrequest);
            }
            catch (AmazonS3Exception ex)
            {
                throw ex;
            }
            catch (WebException ex)
            {
                throw ex;
            }


        }

        /// <summary>
        /// Deletes files from Solera S3 service
        /// </summary>
        /// <param name="deleteFileRequest">Paths in the target bucket</param>
        /// <returns></returns>
        [Consumes("application/json")]
        [HttpDelete]
        public async Task<object> DeleteFileFromSoleraS3(DeleteFileRequest deleteFileRequest)
        {
            _logger.LogInformation($"{JsonConvert.SerializeObject(deleteFileRequest)}");
            AmazonS3Config config = new AmazonS3Config();
            config.ServiceURL = _s3url;
            IAmazonS3 _s3Client = new AmazonS3Client(_s3AccessKey, _s3SecretKey, config);

            DeleteObjectsRequest request = new DeleteObjectsRequest();
            request.BucketName = _s3bucket;
            request.Objects = deleteFileRequest.Body.TargetPaths.Select(t => new KeyVersion() { Key = t }).ToList();
            try
            {
                var response = await _s3Client.DeleteObjectsAsync(request);
                var message = $"Successfully deleted {request.Objects.Count} item(s)";
                _logger.LogInformation(message);
                return StatusCode((int)response.HttpStatusCode,
                    new Response<string>(
                        response.DeleteErrors.Count == 0,
                        (int)response.HttpStatusCode,
                        message));
            }
            catch (DeleteObjectsException e)
            {
                var response = GetErrorResponse(e);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        private Response<string> GetErrorResponse(DeleteObjectsException e)
        {
            var message = $"No. of objects successfully deleted = {e.Response.DeletedObjects.Count}\r\n" +
                        $"No. of objects failed to delete = { e.Response.DeleteErrors.Count}";
            _logger.LogError(message);
            var errors = new List<Error>();
            foreach (DeleteError deleteError in e.Response.DeleteErrors)
            {
                var error = new Error("TargetPaths", $"Object Key: {deleteError.Key}\t{deleteError.Code}\t{deleteError.Message}");
                errors.Add(error);
                _logger.LogError(error.Message);
            }
            return new Response<string>(false, StatusCodes.Status500InternalServerError, null, message, errors.ToArray());
        }
    }
}
