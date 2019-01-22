using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Solera.MediaInfo.Service.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Solera.MediaInfo.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaInfoController : ControllerBase
    {
        // POST: api/MediaInfo
        [HttpPost("photo")]
        public async Task<object> PostPhoto([FromForm] PhotoUploadRequest request)
        {
            BasicAWSCredentials credentials = new BasicAWSCredentials("AKIAID3MAMFSUEUCVWFQ", "4THQSOA10tKIy3yxjWYz1jLOodcsgcVUXZVrycgS");
            IAmazonS3 _s3Client = new AmazonS3Client(credentials, RegionEndpoint.USEast1);

            //This code needs to be moved to the actual externalApi section and through Interfaces and dependency Injection.
            // Just that I am trying the possibility of moving this code to Content delivery service instead of creating another service
            // If that plan falls apart and if we need to maintain this service, this code needs to be refactored accordingly. Whenever that
            // happens, we need the bucket name to be read probably from the environment variable.

            try
            {
                using (var memStream = new MemoryStream())
                {
                    request.photo.CopyTo(memStream);

                    Console.WriteLine("POST PHOTO : Photo data copied to MemoryStream object!");
                    var fileTransferUtility = new TransferUtility(_s3Client);

                    await fileTransferUtility.UploadAsync(memStream,
                                               "gotimedrivervideos/" + request.ValuationNumber, request.FileName);
                    return StatusCode(StatusCodes.Status200OK, "Photo uploaded successful!!");
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status200OK, $"Photo uploaded failed with error {e.Message}!!");
            }
        }
    }
}
