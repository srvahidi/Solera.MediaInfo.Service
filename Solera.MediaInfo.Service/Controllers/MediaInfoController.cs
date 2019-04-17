using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;
using Amazon.S3.Util;// is this needed??
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Solera.MediaInfo.Service.Helpers;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

namespace Solera.MediaInfo.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaInfoController : ControllerBase
    {
        #region Private members
        private readonly string _s3AccessKey;
        private readonly string _s3SecretKey;
        private readonly string _s3url;
        private readonly string _s3bucket;
        private const string NullEmptyArgumentMessage = "Cannot be null or empty.";
        #endregion

        public MediaInfoController()
        {
            _s3AccessKey = Environment.GetEnvironmentVariable("S3_ACCESS_KEY");
            _s3SecretKey = Environment.GetEnvironmentVariable("S3_SECRET_KEY");
            _s3url = Environment.GetEnvironmentVariable("S3_URL");
            _s3bucket = Environment.GetEnvironmentVariable("S3_BUCKET");
        }

        /// <summary>
        /// Upload file to Solera S3 service
        /// </summary>
        /// <param name="file"></param>
        /// <param name="targetPath">file full name in the target bucket</param>
        /// <returns></returns>
        // POST: api/MediaInfo
        [HttpPost("file")]
        public async Task<object> PostFileToSoleraS3([FromForm] string targetPath, IFormFile file)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(targetPath))
                {
                    throw new ArgumentNullException(nameof(targetPath), NullEmptyArgumentMessage);
                }

                if (file == null || file.Length == 0)
                {
                    throw new ArgumentNullException(nameof(file), NullEmptyArgumentMessage);
                }

                AmazonS3Config config = new AmazonS3Config();
                config.ServiceURL = _s3url;
                IAmazonS3 _s3Client = new AmazonS3Client(_s3AccessKey, _s3SecretKey, config);

                StringBuilder bucketPath = new StringBuilder();
                bucketPath.Append(_s3bucket).Append(targetPath).ToString();
                using (var memStream = new MemoryStream())
                {
                    file.CopyTo(memStream);
                    var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = bucketPath.ToString(),
                        InputStream = memStream,
                        StorageClass = S3StorageClass.StandardInfrequentAccess,
                        PartSize = 6291456, // 6 MB.
                        Key = file.FileName,
                        CannedACL = S3CannedACL.PublicRead
                    };

                    var fileTransferUtility = new TransferUtility(_s3Client);
                    Logging.LogInformation("POST S4 : The UploadAsync is invoked with memsetream length: {0}, S3 Bucket: {1}, S3 target file: {2}",
                        memStream.Length, _s3bucket, targetPath);
                    await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);

                    string imageUrl = $"{ _s3url}/{_s3bucket}{targetPath}/{file.FileName}";
                    Console.WriteLine($"Returned Url is {imageUrl}");
                    return StatusCode(StatusCodes.Status200OK, imageUrl);
                }
            }
            catch (Exception e)
            {
                Logging.LogInformation("POST S4 : The file upload failed with the error {0}",
                    e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    IsSuccess = false,
                    Message = $"Error: {e.Message}"
                });
            }
        }
    }
}
