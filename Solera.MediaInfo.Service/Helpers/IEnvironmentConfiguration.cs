using Microsoft.Extensions.Configuration;
using Solera.MediaInfo.Service.Constants;
using System;

namespace Solera.MediaInfo.Service.Helpers
{
    public interface IEnvironmentConfiguration
    {
        ushort ResiliencePolicyMinWaitTimeInMsecs { get; }

        /// <summary>
        /// 
        /// </summary>
        ushort ResiliencePolicyMaxWaitTimeInMsecs { get; }

        /// <summary>
        /// 
        /// </summary>
        ushort ResiliencePolicyMaxRetryCount { get; }



    }
    public class EnvironmentConfiguration : IEnvironmentConfiguration
    {
        public EnvironmentConfiguration(IConfiguration configuration)
        {
            ResiliencePolicyMinWaitTimeInMsecs = Convert.ToUInt16(Environment.GetEnvironmentVariable(EnvironmentVariable.RESILIENCE_POLICY_MIN_WAIT_TIME_MSECS));
            ResiliencePolicyMaxWaitTimeInMsecs = Convert.ToUInt16(Environment.GetEnvironmentVariable(EnvironmentVariable.RESILIENCE_POLICY_MAX_WAIT_TIME_MSECS));
            ResiliencePolicyMaxRetryCount = Convert.ToUInt16(Environment.GetEnvironmentVariable(EnvironmentVariable.RESILIENCE_POLICY_MAX_RETRY_COUNT));

        }


        public ushort ResiliencePolicyMinWaitTimeInMsecs { get; }

        /// <summary>
        /// 
        /// </summary>
        public ushort ResiliencePolicyMaxWaitTimeInMsecs { get; }

        /// <summary>
        /// 
        /// </summary>
        public ushort ResiliencePolicyMaxRetryCount { get; }



    }
}
