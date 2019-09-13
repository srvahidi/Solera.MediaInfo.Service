using Amazon.Runtime;
using System;

namespace Solera.MediaInfo.Service.Helpers
{
    public class EnvironmentVariablesS3Credentials : AWSCredentials
    {
        public override ImmutableCredentials GetCredentials()
        {
            var s3AccessKey = Environment.GetEnvironmentVariable("S3_ACCESS_KEY");
            var s3SecretKey = Environment.GetEnvironmentVariable("S3_SECRET_KEY");
            return new ImmutableCredentials(s3AccessKey, s3SecretKey, null);
        }
    }
}
