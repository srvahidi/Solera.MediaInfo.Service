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
        // GET: api/MediaInfo
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/MediaInfo/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/MediaInfo
        [HttpPost("photo")]
        public async Task<object> PostPhoto([FromForm] PhotoUploadRequest request)
        {
            BasicAWSCredentials credentials = new BasicAWSCredentials("AKIAID3MAMFSUEUCVWFQ", "4THQSOA10tKIy3yxjWYz1jLOodcsgcVUXZVrycgS");
            IAmazonS3 _s3Client = new AmazonS3Client(credentials, RegionEndpoint.USEast1);

            try
            {
                byte[] imageArray = System.IO.File.ReadAllBytes(@"C:\\temp\\bugatti-veyron-2011.jpg");
                //using (Image image = Image.Jpeg("C:\\temp\\bugatti-veyron-2011.jpg"))
                //{
                //    using (MemoryStream m = new MemoryStream())
                //    {
                //        image.Save(m, image.RawFormat);
                //        byte[] imageBytes = m.ToArray();

                //        // Convert byte[] to Base64 String
                //        string base64String = Convert.ToBase64String(imageBytes);
                //        return base64String;
                //    }
                //}
                string base64ImageRepresentation = Convert.ToBase64String(imageArray);

                var bytes = Convert.FromBase64String(base64ImageRepresentation);

                var memst = new MemoryStream(bytes);
                var contents = new StreamContent(new MemoryStream(bytes));

                //var img = Image.(new MemoryStream(Convert.FromBase64String(base64ImageRepresentation)));

                var fileTransferUtility =
                    new TransferUtility(_s3Client);

                //fileTransferUtility.UploadDirectoryAsync()
                //await fileTransferUtility.UploadDirectoryAsync("Claims",                    "gotimedrivervideos");
                //// Option 1. Upload a file. The file name is used as the object key name.
                //await fileTransferUtility.UploadAsync("C:\\temp\\bugatti-veyron-2011.jpg", "gotimedrivervideos" + "/09-98AE-12U34");
                //Console.WriteLine("Upload 1 completed");


                // Option 2. Specify object key name explicitly.
                //await fileTransferUtility.UploadAsync("C:\\temp\\bugatti-veyron-2011.jpg", "gotimedrivervideos", "AKIAID3MAMFSUEUCVWFQ");
                //await fileTransferUtility.UploadAsync("C:\\temp\\bugatti-veyron-2011.jpg", "gotimedrivervideos", "4THQSOA10tKIy3yxjWYz1jLOodcsgcVUXZVrycgS");

                //Console.WriteLine("Upload 2 completed");

                // Option 3. Upload data from a type of System.IO.Stream.
                using (var fileToUpload =
                    new FileStream(@"C:\temp\bugatti-veyron-2011.jpg", FileMode.Open, FileAccess.Read))
                {

                    await fileTransferUtility.UploadAsync(memst,
                                               "gotimedrivervideos" + "/09-98AE-12U33", "bugatti-veyron-201xx.jpg");
                }
                //Console.WriteLine("Upload 3 completed");

                //// Option 4. Specify advanced settings.
                //var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                //{
                //    BucketName = bucketName,
                //    FilePath = filePath,
                //    StorageClass = S3StorageClass.StandardInfrequentAccess,
                //    PartSize = 6291456, // 6 MB.
                //    Key = keyName,
                //    CannedACL = S3CannedACL.PublicRead
                //};
                //fileTransferUtilityRequest.Metadata.Add("param1", "Value1");
                //fileTransferUtilityRequest.Metadata.Add("param2", "Value2");

                //await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
                //Console.WriteLine("Upload 4 completed");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            return null;
        }

        // PUT: api/MediaInfo/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
