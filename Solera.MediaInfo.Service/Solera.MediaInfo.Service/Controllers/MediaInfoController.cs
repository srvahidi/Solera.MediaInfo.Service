using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Solera.MediaInfo.Service.Helpers;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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


                using (var memStream = new MemoryStream())
                {
                    Logging.LogInformation("POST S4 : File data is being copied to memory stream, size {0}",
                        file.Length);
                    file.CopyTo(memStream);
                    Logging.LogInformation("POST S4 : File data copied to memory stream object!");
                    var fileTransferUtility = new TransferUtility(_s3Client);
                    Logging.LogInformation("POST S4 : The UploadAsync is invoked with memsetream length: {0}, S3 Bucket: {1}, S3 target file: {2}",
                        memStream.Length, _s3bucket, targetPath);
                    await fileTransferUtility.UploadAsync(memStream, _s3bucket, targetPath);
                    Logging.LogInformation("POST S4 : File uploaded successfully to: {0}/{1}",
                        _s3bucket, targetPath);
                    return StatusCode(StatusCodes.Status200OK, "File uploaded successfully!!");
                }
            }
            catch (Exception e)
            {
                Logging.LogInformation("POST S4 : The file upload failed with the error {1}",
                    e.Message);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(e.Message),
                    ReasonPhrase = e.Message
                };
            }
        }
    }
}
