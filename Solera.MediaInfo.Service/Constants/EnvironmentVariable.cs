namespace Solera.MediaInfo.Service.Constants
{
    public static class EnvironmentVariable
    {
        public const string
            S3_ACCESS_KEY = "S3_ACCESS_KEY",
            S3_URL = "S3_URL",
            S3_SECRET_KEY = "S3_SECRET_KEY",
            S3_BUCKET = "S3_BUCKET",
            RESILIENCE_POLICY_MIN_WAIT_TIME_MSECS = "RESILIENCE_POLICY_MIN_WAIT_TIME_MSECS",
            RESILIENCE_POLICY_MAX_WAIT_TIME_MSECS = "RESILIENCE_POLICY_MAX_WAIT_TIME_MSECS",
            RESILIENCE_POLICY_MAX_RETRY_COUNT = "RESILIENCE_POLICY_MAX_RETRY_COUNT",
            ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";
    }
}
