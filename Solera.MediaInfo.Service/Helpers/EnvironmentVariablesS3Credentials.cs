using Amazon.Runtime;
using Solera.MediaInfo.Service.Constants;
using System;

namespace Solera.MediaInfo.Service.Helpers
{
    public class EnvironmentVariablesS3Credentials : AWSCredentials
    {
        public override ImmutableCredentials GetCredentials()
        {
            var s3AccessKey = Environment.GetEnvironmentVariable(EnvironmentVariable.S3_ACCESS_KEY);
            var s3SecretKey = Environment.GetEnvironmentVariable(EnvironmentVariable.S3_SECRET_KEY);
            return new ImmutableCredentials(s3AccessKey, s3SecretKey, null);
        }
    }
}
